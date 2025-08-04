# Online Auction System with Real-Time Bidding

## Overview
This Online Auction System is a full-stack platform that enables users to participate in real-time auctions with dynamic bidding and live updates. Built with ASP.NET Core, SignalR, Angular, Redis, and PostgreSQL, the system ensures a smooth, low-latency user experience. It also includes secure authentication, administrative auction controls, and simulated payment handling for complete end-to-end functionality.

## Features
- **Real-Time Bidding**: Built with SignalR for live bid propagation without page refreshes.
- **Dynamic Auction Closing**: Auctions automatically finalize at expiration time or based on rules.
- **User Authentication**: JWT-secured login and registration system.
- **Bid History**: Stores real-time bidding activity with timestamps.
- **Admin Tools**: Admin-only controls to create, edit, and finalize auctions.
- **Redis Pub/Sub**: Scalable architecture supporting multi-instance bid propagation.
- **PostgreSQL Integration**: Persistent storage for users, auctions, and bids.
- **Payment Simulation**: Placeholder logic for checkout flow and winner confirmation.

## Technologies Used
- **Frontend**: Angular
- **Backend**: ASP.NET Core Web API, SignalR
- **Database**: PostgreSQL
- **Real-Time**: SignalR, Redis Pub/Sub
- **Authentication**: JWT (JSON Web Tokens)
- **Other Tools**: Entity Framework Core, Swagger, Docker (optional), xUnit for testing

## Prerequisites
Ensure the following tools are installed:
- .NET SDK 7.0 or later
- Node.js (v18 or later)
- PostgreSQL
- Redis
- Docker (optional)
- Git

