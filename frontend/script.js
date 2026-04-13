const baseUrl = "https://localhost:7296/api/User";

function showLogin() {
  document.getElementById("loginBox").classList.remove("hidden");
  document.getElementById("registerBox").classList.add("hidden");
}

function showRegister() {
  document.getElementById("registerBox").classList.remove("hidden");
  document.getElementById("loginBox").classList.add("hidden");
}

function showSuccess(message) {
  const msg = document.getElementById("msg");
  msg.innerText = message;
  msg.style.color = "green";
  msg.style.background = "#d4edda";
}

function showError(message) {
  const msg = document.getElementById("msg");
  msg.innerText = message;
  msg.style.color = "red";
  msg.style.background = "#f8d7da";
}

window.onload = function () {

  if (window.location.pathname.includes("dashboard.html")) {

    const userId = localStorage.getItem("userId");

    if (!userId) {
      window.location.href = "index.html";
      return;
    }

    const welcome = document.getElementById("welcome");
    if (welcome) {
      welcome.innerText = "Welcome User ID: " + userId;
    }
    loadBalance();
    loadTransactions();
  }

  if (window.location.pathname.includes("index.html") || window.location.pathname === "/") {
    showLogin();
  }
};

function register() {
  const nameVal = document.getElementById("name").value;
  const emailVal = document.getElementById("email").value;
  const passwordVal = document.getElementById("password").value;

  fetch(baseUrl + "/register", {
    method: "POST",
    headers: {"Content-Type": "application/json"},
    body: JSON.stringify({
      name: nameVal,
      email: emailVal,
      password: passwordVal
    })
  })
  .then(async res => {
    const text = await res.text();
    if (!res.ok) throw new Error(text);
    return text;
  })
  .then(() => {
    document.getElementById("msg").innerText = "Registered successfully ✔";
    showLogin(); 
  })
  .catch(err => {
    document.getElementById("msg").innerText = err.message;
  });
}

function login() {
  const emailVal = document.getElementById("loginEmail").value;
  const passwordVal = document.getElementById("loginPassword").value;

  fetch(baseUrl + "/login", {
    method: "POST",
    headers: {"Content-Type": "application/json"},
    body: JSON.stringify({
      email: emailVal,
      password: passwordVal
    })
  })
  .then(res => {
    if (!res.ok) throw new Error("Invalid login");
    return res.json();
  })
  .then(data => {
    localStorage.setItem("userId", data.id);
    window.location.href = "dashboard.html";
  })
  .catch(err => {
    document.getElementById("msg").innerText = err.message;
  });
}

function addMoney() {
  const userId = localStorage.getItem("userId");
  const amount = document.getElementById("amount").value;

  fetch(`${baseUrl}/add-money?userId=${userId}&amount=${amount}`, {
    method: "POST"
  })
  .then(res => {
    if (!res.ok) throw new Error("Failed to add money");
    return res.json();
  })
  .then(() => {
    showSuccess("Money added successfully");
    loadBalance();
    loadTransactions();
  })
  .catch(err => showError(err.message));
}

function transfer() {
  const senderId = localStorage.getItem("userId");
  const receiverId = document.getElementById("receiverId").value;
  const amount = document.getElementById("tamount").value;

  fetch(`${baseUrl}/transfer?senderId=${senderId}&receiverId=${receiverId}&amount=${amount}`, {
    method: "POST"
  })
  .then(res => {
    if (!res.ok) throw new Error("Transfer failed");
    return res.json();
  })
  .then(() => {
    showSuccess("Transfer successful");
    loadBalance();
    loadTransactions();
  })
  .catch(err => showError(err.message));
}

function redeem() {
  const userId = localStorage.getItem("userId");

  fetch(`${baseUrl}/redeem?userId=${userId}`, {
    method: "POST"
  })
  .then(res => {
    if (!res.ok) throw new Error("Not enough points");
    return res.json();
  })
  .then(() => {
    showSuccess("Redeemed successfully");
    loadBalance();
    loadTransactions();
  })
  .catch(err => showError(err.message));
}

function toggleHistory() {
  const history = document.getElementById("history");
  const title = history.previousElementSibling;

  if (history.classList.contains("hidden")) {
    history.classList.remove("hidden");
    title.innerText = "Transaction History ⬆";
  } else {
    history.classList.add("hidden");
    title.innerText = "Transaction History ⬇";
  }
}

function loadBalance() {
  const userId = localStorage.getItem("userId");

  fetch(`${baseUrl}/balance?userId=${userId}`)
    .then(res => res.json())
    .then(data => {
      document.getElementById("balance").innerText =
        "Balance: ₹ " + data;
    })
    .catch(() => showError("Failed to load balance"));
}

function loadTransactions() {
  const userId = localStorage.getItem("userId");

  fetch(`${baseUrl}/transactions?userId=${userId}`)
    .then(res => res.json())
    .then(data => {
      const list = document.getElementById("history");
      list.innerHTML = "";

      data.forEach(t => {
        const li = document.createElement("li");
        li.innerText =
          `From: ${t.senderId} → To: ${t.receiverId} | ₹${t.amount}`;
        list.appendChild(li);
      });
    })
    .catch(() => showError("Failed to load transactions"));
}

function logout() {
  localStorage.removeItem("userId");
  window.location.href = "index.html";
}