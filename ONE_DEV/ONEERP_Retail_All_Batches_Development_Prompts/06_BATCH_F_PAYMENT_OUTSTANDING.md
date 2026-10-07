# Batch F — Payment / Outstanding

## Features
1 Customer Outstanding
2 Payment Allocation
3 Customer Payment History

## Dependency order
Sales Invoice → Payment → Payment Allocation → Outstanding → Customer History

## UI functional checklist
1 Customer Outstanding
2 Payment Entry
3 Payment Allocation
4 Customer Payment History
5 Invoice Payment History
6 Outstanding Settlement

Use actual Payment and PaymentAllocation schema and existing payment APIs. Reconcile invoice totals against paid + balance and validate allocations.

For every feature use the mandatory master format.
