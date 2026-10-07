# Batch E — Returns / Exchange / Refund

## Features
1 Sales Return
2 Sales Return Entry
3 Exchange
4 Refund
5 Credit Note
6 Sales Cancellation / Reverse

## Dependency order
Original Sale → Return → Stock reversal → Exchange/Refund → Credit Note → Cancellation/Reverse

## UI functional checklist
1 Sales Return Register
2 Sales Return Entry
3 Return Detail
4 Exchange Entry
5 Refund Entry
6 Credit Note
7 Sales Cancellation / Reverse
8 Return / Exchange History

Do not invent SalesReturn/CreditNote tables. Inspect actual schema first. Preserve transaction integrity and rollback related stock/payment effects on failure.

For every feature use the mandatory master format.
