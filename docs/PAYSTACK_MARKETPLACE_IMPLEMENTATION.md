# Atelier.PlanMorph Payment Architecture (Kenya, KES)

This implementation package provides:
- Database schema for users, wallets, transactions, withdrawals, commission tiers
- Founding member logic (first 25 architects + first 25 engineers)
- Tiered commission calculation (design sales and contract referral)
- Paystack split initialization approach
- Idempotent and atomic withdrawal flow
- Webhook signature verification pattern

Files:
- examples/node-payments/schema.sql
- examples/node-payments/index.js
- examples/node-payments/package.json

## 1) Commission Rules Implemented

### Founding members
- Verified Architect/Engineer users can be flagged as founding members.
- Founding members pay 0% on design sales.

### Design commission tiers
- 0 - 20,000 KES: 3%
- 20,001 - 50,000 KES: 4%
- 50,001 - 100,000 KES: 5%
- 100,001 - 200,000 KES: 6%
- 200,001+ KES: 7%

### Contract referral commission tiers
- 0 - 500,000 KES: 1.5%
- 500,001 - 2,000,000 KES: 2%
- 2,000,001 - 10,000,000 KES: 2.5%
- 10,000,001+ KES: 3%

## 2) Wallet Ledger Rules

For each wallet:
- total_earned
- total_withdrawn
- pending_balance
- available_balance = total_earned - total_withdrawn - pending_balance

Withdrawal guards:
- No withdrawal when available_balance <= 0
- No withdrawal above available_balance
- Amount is first locked to pending_balance
- On transfer success: move lock to total_withdrawn
- On transfer failure: release lock

## 3) Atomic Withdrawal + Race Prevention

In index.js:
- Wallet row is locked using SELECT ... FOR UPDATE
- Writes are wrapped with database transactions
- Idempotency key is required and unique in withdrawals table
- Duplicate idempotency key returns already-processed result

## 4) Paystack Split Strategy

For design purchase initialization:
- Uses transaction initialize with:
  - subaccount = professional subaccount code
  - transaction_charge = company commission amount (in kobo)
  - bearer = subaccount

This routes commission to the company and remainder to the professional subaccount.

## 5) Webhook Security

Webhook endpoint:
- Validates x-paystack-signature using HMAC SHA-512
- Stores events idempotently in paystack_events
- Ignores duplicates by event_id unique key

## 6) Founding Member Assignment Pattern

Operationally, assign founding slots in admin workflow:
1. Ensure user role is architect or engineer
2. Ensure user is verified
3. Count existing founding members by role
4. If role count < slot limit (25), set:
   - is_founding_member = true
   - founding_member_slot = next slot number
5. Save in a transaction

## 7) Run Example

1. Install:
- cd examples/node-payments
- npm install

2. Configure env (from root .env section)

3. Run:
- npm run dev

4. Endpoints:
- POST /payments/design/initialize
- POST /withdrawals/request
- POST /webhooks/paystack
- GET /health

## 8) Notes for Production

- Add strict authz middleware on payment endpoints
- Add per-user withdrawal rate limits and velocity checks
- Encrypt sensitive account details at rest
- Restrict webhook endpoint by signature and optional IP allowlist
- Add reconciliation jobs for transfer finality and disputes
- Build admin dispute override UI backed by financial_audit_logs
