# Billing Service - Angular UI

Frontend for the BillingService API: login, manage products/customers,
create orders, record payments (cash/card/MFS), and download invoice PDFs.

## Setup

```bash
npm install
npm start
```

Opens at http://localhost:4200. Make sure the BillingService API is running
first (defaults to http://localhost:5000 - check `src/environments/environment.ts`
if yours is on a different port).

## Login

Use your seeded admin account (or whichever account you've created):
- Email: admin@billingservice.local
- Password: Admin@12345

## Notes

- There's currently no "list all orders" endpoint on the API, so the Orders
  page works by order ID lookup rather than a full list. Worth adding a
  `GET /api/Orders` endpoint later if you want a proper order history view.
- Roles: Admin/Manager can create products and customers; Admin/Manager/Cashier
  can create orders and record payments. The UI hides buttons accordingly,
  but the API also enforces this independently.
