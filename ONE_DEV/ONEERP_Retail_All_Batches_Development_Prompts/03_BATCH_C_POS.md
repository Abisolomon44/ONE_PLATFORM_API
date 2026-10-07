# Batch C — POS

## Features
1 POS Hold
2 POS Recall
3 POS Cash In / Cash Out
4 POS Day Close / Settlement
5 POS Exchange
6 POS Refund

## Dependency order
POS Session → Hold/Recall → Cash Movement → Settlement → Exchange/Refund

## UI functional checklist
1 POS Sales Screen
2 Hold Sale
3 Recall Sale
4 POS Cash In
5 POS Cash Out
6 POS Day Close
7 POS Settlement
8 POS Exchange
9 POS Refund
10 POS Session History

Use the existing Company → Branch → Store → Counter → Counter Assignment → Operator → POS Session → Sales Invoice hierarchy. Backend validates POS context; do not trust frontend context.

For every feature use the mandatory master format.
