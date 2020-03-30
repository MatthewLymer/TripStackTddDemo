# TripStackTddDemo

This example solution is to show how to implement caching functionality into an application using Test Driven Development (TDD).

The current application comprises of two ASP.NET Web API applications, `TripStack.TddDemo.StoreApi` intended to be a public-facing API, and `TripStack.TddDemo.CurrencyExchangeApi` representing a 3rd-party service that requires authentication and has rate-limiting.

## TripStack.TddDemo.StoreApi

This application allows the user to view a list of products from all over the world, with pricing from their region of origin.  

To be helpful to our users, we allow them to specify the currency code in their request to normalize the currencies to what is requested.

```
curl -X GET 'http://localhost:5000/Products?currencyCode=CAD'
```

Where `currencyCode` is optional (defaulting to `USD`), or be one of the following: `USD`, `CAD`, `EUR`, `GBP`, or `MXN`

## TripStack.TddDemo.CurrencyExchangeApi

This application allows the user to get a conversion factor between two currencies.  It is intended to simulate a 3rd-party service that we do not control.  In addition to this, it is rate limited to only allowing **15 requests / minute**

The endpoint is also configured for authentication to ensure no unauthorized access, and to ensure the rate limit is per-tenant.

```
curl -X GET -H 'authorization: Bearer 71FE92A8-E54B-4E99-A130-D960DCD0436E' 'http://localhost:5100/rates?from=USD&to=CAD'
```

# The Problem

While the integration between the Store API and the CurrencyExchange API works, if too many requests to the currency exchange service are made then it will return an HTTP response `429 - Too many requests`, and subsequently our Store API request will fail.

# The Solution

With the help of TDD, we can implement a caching layer for our currency exchange service to prevent hitting our **15 requests / minute** limit.

The application has also been setup to leverage Redis as a cache.