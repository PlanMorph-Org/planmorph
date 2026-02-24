-- Kenya / KES multi-vendor marketplace schema (PostgreSQL)
-- Covers: users, founding members, commission tiers, wallets, ledger txns, withdrawals, webhooks, audit logs

create type user_role as enum ('architect', 'engineer', 'contractor', 'client', 'admin');
create type revenue_type as enum ('design_sale', 'contract_referral');
create type ledger_txn_type as enum (
  'credit_earned',
  'debit_withdrawn',
  'lock_withdrawal',
  'unlock_withdrawal',
  'platform_commission',
  'adjustment'
);
create type withdrawal_status as enum ('pending', 'processing', 'completed', 'failed', 'cancelled');
create type paystack_event_status as enum ('received', 'processed', 'ignored', 'failed');

create table if not exists users (
  id uuid primary key,
  email text not null unique,
  role user_role not null,
  is_verified boolean not null default false,
  is_active boolean not null default true,

  -- founding member logic
  is_founding_member boolean not null default false,
  founding_member_slot integer,

  -- Paystack destination for split/transfer
  paystack_subaccount_code text,
  paystack_recipient_code text,

  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists commission_tiers (
  id bigserial primary key,
  revenue_type revenue_type not null,
  min_amount_kes numeric(18,2) not null,
  max_amount_kes numeric(18,2),
  rate_percent numeric(5,2) not null,
  is_active boolean not null default true,
  created_at timestamptz not null default now(),
  constraint chk_commission_rate check (rate_percent >= 0 and rate_percent <= 100),
  constraint chk_commission_range check (max_amount_kes is null or max_amount_kes >= min_amount_kes)
);
create index if not exists ix_commission_tiers_lookup
  on commission_tiers (revenue_type, is_active, min_amount_kes, max_amount_kes);

create table if not exists wallets (
  id uuid primary key,
  user_id uuid not null unique references users(id),
  currency char(3) not null default 'KES',

  total_earned numeric(18,2) not null default 0,
  total_withdrawn numeric(18,2) not null default 0,
  pending_balance numeric(18,2) not null default 0,

  -- generated for consistency (or compute in query if preferred)
  available_balance numeric(18,2) generated always as (total_earned - total_withdrawn - pending_balance) stored,

  row_version bigint not null default 0,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),

  constraint chk_wallet_nonnegative check (
    total_earned >= 0 and total_withdrawn >= 0 and pending_balance >= 0
  )
);

create table if not exists wallet_transactions (
  id uuid primary key,
  wallet_id uuid not null references wallets(id),
  user_id uuid not null references users(id),
  txn_type ledger_txn_type not null,
  amount numeric(18,2) not null,
  balance_before numeric(18,2) not null,
  balance_after numeric(18,2) not null,
  currency char(3) not null default 'KES',

  -- traceability/idempotency
  idempotency_key text,
  external_ref text,
  related_withdrawal_id uuid,
  metadata jsonb,

  created_at timestamptz not null default now(),
  constraint chk_wallet_txn_amount_nonzero check (amount <> 0)
);
create unique index if not exists ux_wallet_txn_idempotency
  on wallet_transactions (idempotency_key)
  where idempotency_key is not null;

create table if not exists withdrawals (
  id uuid primary key,
  user_id uuid not null references users(id),
  wallet_id uuid not null references wallets(id),

  requested_amount numeric(18,2) not null,
  locked_amount numeric(18,2) not null,
  fee_amount numeric(18,2) not null default 0,
  net_transfer_amount numeric(18,2) not null,
  currency char(3) not null default 'KES',

  status withdrawal_status not null default 'pending',
  failure_reason text,

  -- Paystack idempotency + transfer trace
  idempotency_key text not null,
  paystack_transfer_code text,
  paystack_reference text,
  paystack_recipient_code text,

  requested_at timestamptz not null default now(),
  processed_at timestamptz,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),

  constraint chk_withdrawal_positive check (requested_amount > 0 and locked_amount > 0 and net_transfer_amount > 0)
);
create unique index if not exists ux_withdrawal_idempotency on withdrawals(idempotency_key);
create index if not exists ix_withdrawals_user_status on withdrawals(user_id, status);

create table if not exists paystack_events (
  id bigserial primary key,
  event_id text,
  event_type text not null,
  event_signature text,
  payload jsonb not null,
  status paystack_event_status not null default 'received',
  error_message text,
  received_at timestamptz not null default now(),
  processed_at timestamptz
);
create unique index if not exists ux_paystack_event_id on paystack_events(event_id) where event_id is not null;

create table if not exists financial_audit_logs (
  id bigserial primary key,
  actor_user_id uuid,
  target_user_id uuid,
  action text not null,
  reference text,
  details jsonb,
  created_at timestamptz not null default now()
);

-- seed default commission tiers
insert into commission_tiers (revenue_type, min_amount_kes, max_amount_kes, rate_percent)
values
  ('design_sale', 0, 20000, 3.00),
  ('design_sale', 20001, 50000, 4.00),
  ('design_sale', 50001, 100000, 5.00),
  ('design_sale', 100001, 200000, 6.00),
  ('design_sale', 200001, null, 7.00),

  ('contract_referral', 0, 500000, 1.50),
  ('contract_referral', 500001, 2000000, 2.00),
  ('contract_referral', 2000001, 10000000, 2.50),
  ('contract_referral', 10000001, null, 3.00)
on conflict do nothing;
