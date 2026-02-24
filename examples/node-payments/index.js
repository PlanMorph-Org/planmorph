import crypto from 'crypto';
import express from 'express';
import helmet from 'helmet';
import morgan from 'morgan';
import axios from 'axios';
import dotenv from 'dotenv';
import { Pool } from 'pg';
import { z } from 'zod';

dotenv.config();

const app = express();
app.use(helmet());
app.use(morgan('combined'));
app.use(express.json({ verify: rawBodySaver }));

const pool = new Pool({ connectionString: process.env.DATABASE_URL });

const cfg = {
  currency: process.env.WALLET_CURRENCY || 'KES',
  paystackBase: process.env.PAYSTACK_BASE_URL || 'https://api.paystack.co',
  paystackSecret: process.env.PAYSTACK_MAIN_SECRET_KEY,
  webhookSecret: process.env.PAYSTACK_WEBHOOK_SECRET,
  foundingArchSlots: Number(process.env.FOUNDING_ARCHITECT_SLOTS || 25),
  foundingEngSlots: Number(process.env.FOUNDING_ENGINEER_SLOTS || 25)
};

if (!cfg.paystackSecret || !cfg.webhookSecret) {
  console.warn('Missing Paystack secrets in environment.');
}

function rawBodySaver(req, _res, buf) {
  req.rawBody = buf;
}

const withdrawalSchema = z.object({
  userId: z.string().uuid(),
  amount: z.number().positive(),
  idempotencyKey: z.string().min(10),
  accountNumber: z.string().min(6),
  bankCode: z.string().min(2)
});

function normalizeRole(role) {
  const r = String(role || '').toLowerCase();
  if (r === 'architect' || r === 'engineer' || r === 'contractor') return r;
  return r;
}

export function isFoundingMember(user) {
  return user?.is_founding_member === true;
}

export function calculateCommission(amountKes, revenueType, user, tiers) {
  if (isFoundingMember(user) && revenueType === 'design_sale') {
    return { ratePercent: 0, commissionAmount: 0 };
  }

  const tier = tiers.find((t) => {
    const minOk = amountKes >= Number(t.min_amount_kes);
    const maxOk = t.max_amount_kes == null ? true : amountKes <= Number(t.max_amount_kes);
    return minOk && maxOk;
  });

  if (!tier) throw new Error(`No commission tier for ${revenueType} amount ${amountKes}`);

  const ratePercent = Number(tier.rate_percent);
  const commissionAmount = Number(((amountKes * ratePercent) / 100).toFixed(2));
  return { ratePercent, commissionAmount };
}

async function getActiveCommissionTiers(client, revenueType) {
  const q = await client.query(
    `select min_amount_kes, max_amount_kes, rate_percent
     from commission_tiers
     where revenue_type = $1 and is_active = true
     order by min_amount_kes asc`,
    [revenueType]
  );
  return q.rows;
}

async function lockWalletForUpdate(client, userId) {
  const q = await client.query(
    `select w.*, u.paystack_recipient_code, u.paystack_subaccount_code
     from wallets w
     join users u on u.id = w.user_id
     where w.user_id = $1
     for update`,
    [userId]
  );
  if (q.rows.length === 0) throw new Error('Wallet not found');
  return q.rows[0];
}

async function createPaystackRecipient({ name, accountNumber, bankCode }) {
  const response = await axios.post(
    `${cfg.paystackBase}/transferrecipient`,
    {
      type: 'nuban',
      name,
      account_number: accountNumber,
      bank_code: bankCode,
      currency: cfg.currency
    },
    {
      headers: { Authorization: `Bearer ${cfg.paystackSecret}` }
    }
  );
  if (!response.data?.status) throw new Error('Paystack recipient creation failed');
  return response.data.data.recipient_code;
}

async function initiatePaystackTransfer({ amount, recipientCode, reference, reason }) {
  const response = await axios.post(
    `${cfg.paystackBase}/transfer`,
    {
      source: 'balance',
      amount: Math.round(amount * 100),
      recipient: recipientCode,
      reference,
      reason
    },
    {
      headers: { Authorization: `Bearer ${cfg.paystackSecret}` }
    }
  );
  if (!response.data?.status) throw new Error('Paystack transfer initiation failed');
  return response.data.data;
}

async function recordAudit(client, action, details, actorUserId = null, targetUserId = null, reference = null) {
  await client.query(
    `insert into financial_audit_logs(actor_user_id, target_user_id, action, reference, details)
     values ($1, $2, $3, $4, $5::jsonb)`,
    [actorUserId, targetUserId, action, reference, JSON.stringify(details || {})]
  );
}

app.post('/payments/design/initialize', async (req, res) => {
  const { buyerEmail, professionalUserId, amountKes, reference } = req.body;

  if (!buyerEmail || !professionalUserId || !amountKes || !reference) {
    return res.status(400).json({ message: 'Missing required fields' });
  }

  const client = await pool.connect();
  try {
    const userQ = await client.query('select id, role, is_founding_member, paystack_subaccount_code from users where id=$1', [professionalUserId]);
    if (userQ.rows.length === 0) return res.status(404).json({ message: 'Professional not found' });

    const professional = userQ.rows[0];
    const role = normalizeRole(professional.role);
    if (role !== 'architect' && role !== 'engineer') {
      return res.status(400).json({ message: 'Target user must be architect or engineer' });
    }

    const tiers = await getActiveCommissionTiers(client, 'design_sale');
    const { ratePercent, commissionAmount } = calculateCommission(Number(amountKes), 'design_sale', professional, tiers);

    // Dynamic split using subaccount and fee
    // Company fee = commissionAmount, vendor gets remainder
    const payload = {
      email: buyerEmail,
      amount: Math.round(Number(amountKes) * 100),
      reference,
      currency: cfg.currency,
      subaccount: professional.paystack_subaccount_code,
      transaction_charge: Math.round(commissionAmount * 100),
      bearer: 'subaccount'
    };

    const response = await axios.post(`${cfg.paystackBase}/transaction/initialize`, payload, {
      headers: { Authorization: `Bearer ${cfg.paystackSecret}` }
    });

    await recordAudit(client, 'design_payment_initialize', {
      professionalUserId,
      amountKes,
      commissionRate: ratePercent,
      commissionAmount,
      reference
    }, null, professionalUserId, reference);

    return res.json({
      reference,
      ratePercent,
      commissionAmount,
      paystack: response.data?.data || null
    });
  } catch (error) {
    return res.status(500).json({ message: 'Initialize payment failed', error: String(error.message || error) });
  } finally {
    client.release();
  }
});

app.post('/withdrawals/request', async (req, res) => {
  const parsed = withdrawalSchema.safeParse(req.body);
  if (!parsed.success) {
    return res.status(400).json({ message: 'Invalid payload', issues: parsed.error.issues });
  }

  const { userId, amount, idempotencyKey, accountNumber, bankCode } = parsed.data;
  const client = await pool.connect();

  try {
    await client.query('begin');

    const exists = await client.query('select id, status from withdrawals where idempotency_key=$1', [idempotencyKey]);
    if (exists.rows.length > 0) {
      await client.query('rollback');
      return res.status(200).json({ message: 'Already processed', withdrawal: exists.rows[0] });
    }

    const wallet = await lockWalletForUpdate(client, userId);
    const available = Number(wallet.available_balance);

    if (available <= 0 || amount > available) {
      await client.query('rollback');
      return res.status(400).json({ message: 'Insufficient available balance', availableBalance: available });
    }

    const withdrawalId = crypto.randomUUID();
    const reference = `PM-WD-${Date.now()}-${Math.floor(Math.random() * 100000)}`;

    const pendingBefore = Number(wallet.pending_balance);
    const pendingAfter = Number((pendingBefore + amount).toFixed(2));

    await client.query(
      `update wallets
       set pending_balance = $1, row_version = row_version + 1, updated_at = now()
       where id = $2`,
      [pendingAfter, wallet.id]
    );

    await client.query(
      `insert into withdrawals(
         id, user_id, wallet_id, requested_amount, locked_amount, net_transfer_amount,
         status, idempotency_key, paystack_reference, paystack_recipient_code
       ) values ($1,$2,$3,$4,$5,$6,'processing',$7,$8,$9)`,
      [withdrawalId, userId, wallet.id, amount, amount, amount, idempotencyKey, reference, wallet.paystack_recipient_code]
    );

    await client.query(
      `insert into wallet_transactions(
         id, wallet_id, user_id, txn_type, amount, balance_before, balance_after, idempotency_key, related_withdrawal_id, external_ref, metadata
       ) values ($1,$2,$3,'lock_withdrawal',$4,$5,$6,$7,$8,$9,$10::jsonb)`,
      [
        crypto.randomUUID(),
        wallet.id,
        userId,
        amount,
        pendingBefore,
        pendingAfter,
        idempotencyKey,
        withdrawalId,
        reference,
        JSON.stringify({ reason: 'withdrawal lock' })
      ]
    );

    await recordAudit(client, 'withdrawal_locked', { userId, withdrawalId, amount, reference }, userId, userId, reference);

    await client.query('commit');

    // external transfer after lock commit
    let recipientCode = wallet.paystack_recipient_code;
    if (!recipientCode) {
      recipientCode = await createPaystackRecipient({
        name: `PlanMorph User ${userId.slice(0, 8)}`,
        accountNumber,
        bankCode
      });

      await pool.query('update users set paystack_recipient_code=$1, updated_at=now() where id=$2', [recipientCode, userId]);
      await pool.query('update withdrawals set paystack_recipient_code=$1 where id=$2', [recipientCode, withdrawalId]);
    }

    try {
      const transfer = await initiatePaystackTransfer({
        amount,
        recipientCode,
        reference,
        reason: 'Atelier.PlanMorph withdrawal'
      });

      const tx2 = await pool.connect();
      try {
        await tx2.query('begin');

        const wallet2 = await lockWalletForUpdate(tx2, userId);

        const totalWithdrawnBefore = Number(wallet2.total_withdrawn);
        const totalWithdrawnAfter = Number((totalWithdrawnBefore + amount).toFixed(2));
        const pendingNow = Number(wallet2.pending_balance);
        const pendingAfterSuccess = Number((pendingNow - amount).toFixed(2));

        await tx2.query(
          `update wallets
           set total_withdrawn=$1, pending_balance=$2, row_version=row_version+1, updated_at=now()
           where id=$3`,
          [totalWithdrawnAfter, pendingAfterSuccess, wallet2.id]
        );

        await tx2.query(
          `update withdrawals
           set status='completed', paystack_transfer_code=$1, processed_at=now(), updated_at=now()
           where id=$2`,
          [transfer.transfer_code || null, withdrawalId]
        );

        await tx2.query(
          `insert into wallet_transactions(
             id, wallet_id, user_id, txn_type, amount, balance_before, balance_after, idempotency_key, related_withdrawal_id, external_ref, metadata
           ) values ($1,$2,$3,'debit_withdrawn',$4,$5,$6,$7,$8,$9,$10::jsonb)`,
          [
            crypto.randomUUID(),
            wallet2.id,
            userId,
            amount,
            totalWithdrawnBefore,
            totalWithdrawnAfter,
            idempotencyKey,
            withdrawalId,
            reference,
            JSON.stringify({ transferCode: transfer.transfer_code || null })
          ]
        );

        await recordAudit(tx2, 'withdrawal_completed', { userId, withdrawalId, amount, reference }, userId, userId, reference);
        await tx2.query('commit');
      } catch (e) {
        await tx2.query('rollback');
        throw e;
      } finally {
        tx2.release();
      }

      return res.json({ message: 'Withdrawal completed', withdrawalId, reference, transferCode: transfer.transfer_code || null });
    } catch (transferError) {
      const tx3 = await pool.connect();
      try {
        await tx3.query('begin');

        const wallet3 = await lockWalletForUpdate(tx3, userId);
        const pendingNow = Number(wallet3.pending_balance);
        const pendingAfterRelease = Number((pendingNow - amount).toFixed(2));

        await tx3.query(
          `update wallets
           set pending_balance=$1, row_version=row_version+1, updated_at=now()
           where id=$2`,
          [pendingAfterRelease, wallet3.id]
        );

        await tx3.query(
          `update withdrawals
           set status='failed', failure_reason=$1, processed_at=now(), updated_at=now()
           where id=$2`,
          [String(transferError.message || transferError), withdrawalId]
        );

        await tx3.query(
          `insert into wallet_transactions(
             id, wallet_id, user_id, txn_type, amount, balance_before, balance_after, idempotency_key, related_withdrawal_id, external_ref, metadata
           ) values ($1,$2,$3,'unlock_withdrawal',$4,$5,$6,$7,$8,$9,$10::jsonb)`,
          [
            crypto.randomUUID(),
            wallet3.id,
            userId,
            amount,
            pendingNow,
            pendingAfterRelease,
            idempotencyKey,
            withdrawalId,
            reference,
            JSON.stringify({ reason: 'transfer failed, unlocked pending' })
          ]
        );

        await recordAudit(tx3, 'withdrawal_failed_unlocked', {
          userId,
          withdrawalId,
          amount,
          reference,
          reason: String(transferError.message || transferError)
        }, userId, userId, reference);

        await tx3.query('commit');
      } catch (e2) {
        await tx3.query('rollback');
        throw e2;
      } finally {
        tx3.release();
      }

      return res.status(502).json({ message: 'Transfer failed; locked amount released', withdrawalId, reference });
    }
  } catch (error) {
    try {
      await client.query('rollback');
    } catch {}
    return res.status(500).json({ message: 'Withdrawal request failed', error: String(error.message || error) });
  } finally {
    client.release();
  }
});

app.post('/webhooks/paystack', async (req, res) => {
  try {
    const signature = req.headers['x-paystack-signature'];
    const expected = crypto
      .createHmac('sha512', cfg.webhookSecret)
      .update(req.rawBody || Buffer.from(JSON.stringify(req.body)))
      .digest('hex');

    if (!signature || signature !== expected) {
      return res.status(401).json({ message: 'Invalid webhook signature' });
    }

    const event = req.body;
    const eventId = event?.data?.id ? String(event.data.id) : null;

    // idempotent event write
    const insert = await pool.query(
      `insert into paystack_events(event_id, event_type, event_signature, payload, status)
       values ($1,$2,$3,$4::jsonb,'received')
       on conflict (event_id) do nothing
       returning id`,
      [eventId, String(event?.event || 'unknown'), String(signature), JSON.stringify(event)]
    );

    if (insert.rowCount === 0) {
      return res.status(200).json({ message: 'Duplicate webhook ignored' });
    }

    // Example: mark processed. Add event-specific handlers here.
    await pool.query(
      `update paystack_events set status='processed', processed_at=now() where id=$1`,
      [insert.rows[0].id]
    );

    return res.status(200).json({ message: 'Webhook accepted' });
  } catch (error) {
    return res.status(500).json({ message: 'Webhook handler failed', error: String(error.message || error) });
  }
});

app.get('/health', (_req, res) => {
  res.json({ ok: true, service: 'planmorph-payments-example' });
});

const port = Number(process.env.PORT || 4004);
app.listen(port, () => {
  console.log(`Payments example listening on :${port}`);
});
