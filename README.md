# 💰 Digital Wallet System

A full-stack web application that allows users to manage their wallet, perform transactions, and earn reward points with secure JWT authentication.

---

## 🚀 Features

### 🔐 Authentication

* User Registration & Login
* JWT-based authentication
* Secure API access

### 💳 Wallet System

* Add money to wallet
* Transfer money between users
* View wallet balance

### 📜 Transactions

* Track transaction history
* View sender & receiver details

### 🎁 Reward Points System

* Earn **1 point for every ₹100 transfer**
* Redeem:

  * **10 points → ₹50 cashback**
  * **50 points → ₹100 cashback**
* Real-time points display on dashboard

---

## 🛠️ Tech Stack

### Backend

* ASP.NET Core Web API
* Entity Framework Core
* MySQL Database

### Frontend

* HTML
* CSS
* JavaScript (Fetch API)

### Authentication

* JWT (JSON Web Tokens)

---

## 📂 Project Structure

DigitalWallet/
│
├── backend/
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   └── Program.cs
│
├── frontend/
│   ├── index.html
│   ├── dashboard.html
│   └── script.js
│
└── README.md

---

## 🔐 API Endpoints

| Method | Endpoint               | Description         |
| ------ | ---------------------- | ------------------- |
| POST   | /api/User/register     | Register user       |
| POST   | /api/User/login        | Login user          |
| POST   | /api/User/add-money    | Add money           |
| POST   | /api/User/transfer     | Transfer money      |
| POST   | /api/User/redeem       | Redeem points       |
| GET    | /api/User/balance      | Get balance         |
| GET    | /api/User/transactions | Transaction history |
| GET    | /api/User/points       | Get reward points   |
| GET    | /api/User/profile      | Get user profile    | 
---

## 🧪 Testing

* Use Swagger UI for backend testing
* Use browser for frontend testing

## 👨‍💻 Author

Ayush Kumar Shukla

---

## ⭐ Acknowledgment

This project was built to demonstrate full-stack development skills including authentication, API integration, and business logic implementation.

