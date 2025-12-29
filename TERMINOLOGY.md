# Brelio Backend Terminology Guide

This document defines the language standards for all public-facing and internal code in the Brelio API.

## Core Principle

Brelio abstracts payment infrastructure. We sell **money movement**, not blockchain ideology.

---

## Public-Facing Language

### Required Terms

| Use This | Instead Of |
|----------|------------|
| Digital dollars | Crypto, cryptocurrency |
| USDC | Stablecoin, token |
| Digital dollar wallet | Crypto wallet |
| Payment address | Wallet address (in error messages) |
| Payment network | Blockchain, Solana |
| Payment | Transaction (when user-facing) |
| Network fees | Gas fees |

### Examples

**Error Messages**
```csharp
// GOOD
throw new ArgumentException("Invalid payment address");

// BAD
throw new ArgumentException("Invalid Solana wallet address");
```

**Plan Descriptions (API Responses)**
```csharp
// GOOD
Description = "Perfect for freelancers getting started with digital dollar invoicing."

// BAD
Description = "Perfect for freelancers getting started with crypto invoicing."
```

**API Documentation / Swagger**
```csharp
/// <summary>
/// Create a new invoice for digital dollar payments
/// </summary>

// Not: "Create a new crypto invoice"
```

---

## Internal-Only Language (Acceptable)

The following terms may be used in internal code that is **never exposed to clients**:

### Allowed Internally

- Class/interface names: `SolanaService`, `ISolanaService`, `SolanaMonitorService`
- Method names: `ValidateSolanaAddress`, `GetRecentUsdcTransactionsAsync`
- Internal comments explaining technical implementation
- Log messages for operations/debugging
- Configuration keys: `Solana:RpcUrl`, `Solana:UsdcMint`
- Entity property names: `TxSignature`, `WalletId`

### Internal Naming Conventions

```csharp
// Internal service - OK
public class SolanaService : ISolanaService

// Internal logging - OK
_logger.LogInformation("Payment detected for invoice {Id}: {Amount} USDC");

// Internal comments - OK
// Solana addresses are Base58 encoded and 32-44 characters
```

---

## What Gets Exposed to Clients

Be vigilant about these areas:

| Layer | Exposed? | Caution Level |
|-------|----------|---------------|
| API response DTOs | Yes | HIGH |
| Error messages (`ex.Message`) | Yes | HIGH |
| OpenAPI/Swagger descriptions | Yes | HIGH |
| Plan/feature descriptions (from DB) | Yes | HIGH |
| Internal service names | No | LOW |
| Log messages | No | LOW |
| Internal comments | No | LOW |
| Entity property names | Partial | MEDIUM |

---

## Database Seed Data

Plan descriptions are returned via the `/api/Plans` endpoint. Ensure all plan descriptions follow public-facing language:

```csharp
// GOOD
Description = "For freelancers getting started with digital dollar invoicing."

// BAD
Description = "For freelancers getting started with crypto invoicing."
```

---

## Quick Reference for Code Review

Before merging, check:

1. [ ] Error messages don't mention "Solana", "blockchain", "crypto"
2. [ ] API response descriptions use "digital dollars" not "crypto"
3. [ ] Swagger/OpenAPI summaries are client-friendly
4. [ ] Database seed data uses approved terminology
5. [ ] Any string that could reach the client follows the guide

---

## Legal Disclosure (Required Once)

In FAQ, Terms, or footer tooltip, include this disclosure:

> USDC is a regulated, dollar-backed digital currency issued by Circle. Payments are processed on a public payment network.

This satisfies transparency requirements without using loaded terminology.

---

## Questions?

When in doubt, ask: "Would a non-technical CFO understand this without needing to know about blockchain?"

If no — rewrite it.
