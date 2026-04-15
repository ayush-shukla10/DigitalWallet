const baseUrl = "https://localhost:7296/api/User";

function getAuthHeader() {
    const token = localStorage.getItem("token");

    if (!token) {
        console.error("Token missing. Please login again.");
        return {};
    }

    return {
        "Authorization": `Bearer ${token}`
    };
}

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
      const name = localStorage.getItem("name");
      welcome.innerText = "Welcome " + name;
    }
    loadBalance();
    loadTransactions();
    loadPoints();
  }

  if (window.location.pathname.includes("index.html") || window.location.pathname === "/") {
    showLogin();
  }
};

function register() {
    const name = document.getElementById("name").value;
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;
    const msg = document.getElementById("msg");

    if (!name || !email || !password) {
        msg.innerText = "All fields are required";
        return;
    }

    if (password.length < 8) {
        msg.innerText = "Password must be at least 8 characters";
        return;
    }

    fetch(baseUrl + "/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, email, password })
    })
    .then(async res => {
        if (!res.ok) {
            const errorText = await res.text();
            throw new Error(errorText);
        }
        return res.json();
    })
    .then(() => {
        msg.innerText = "Registered successfully ✔";
    })
    .catch(err => {
        msg.innerText = err.message;
    });
}

function login() {
    const emailVal = document.getElementById("loginEmail").value;
    const passwordVal = document.getElementById("loginPassword").value;
    const msg = document.getElementById("msg");
    if (!emailVal || !passwordVal) {
        msg.innerText = "Email and password are required";
        return;
    }
    fetch(baseUrl + "/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
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
        localStorage.setItem("token", data.token);
        localStorage.setItem("userId", data.userId);
        localStorage.setItem("name", data.name);

        window.location.href = "dashboard.html";
    })
    .catch(err => {
        msg.innerText = err.message;
    });
}

function addMoney() {
    const userId = localStorage.getItem("userId");
    const amount = document.getElementById("amount").value;

    fetch(`${baseUrl}/add-money?userId=${userId}&amount=${amount}`, {
        method: "POST",
        headers: getAuthHeader()
    })
    .then(res => res.json())
    .then(() => {
        showSuccess("Money added");
        loadBalance();
        loadTransactions();
    })
    .catch(() => showError("Failed"));
}

function transfer() {
    const userId = localStorage.getItem("userId");
    const receiverId = document.getElementById("receiverId").value;
    const amount = document.getElementById("tamount").value;

    fetch(`${baseUrl}/transfer?senderId=${userId}&receiverId=${receiverId}&amount=${amount}`, {
        method: "POST",
        headers: getAuthHeader()
    })
    .then(res => res.json())
    .then(() => {
        showSuccess("Transfer successful");
        loadBalance();
        loadTransactions();
    })
    .catch(() => showError("Failed"));
}

function redeem() {
    const userId = localStorage.getItem("userId");

    fetch(`${baseUrl}/redeem?userId=${userId}`, {
        method: "POST",
        headers: getAuthHeader()
    })
    .then(res => {
        if (!res.ok) {
            return res.text().then(err => { throw new Error(err); });
        }
        return res.json();
    })
    .then(data => {
        showSuccess(data.message);
        loadBalance();
        loadTransactions();
    })
    .catch(err => {
        showError(err.message); 
    });
}

function toggleHistory() {
    const history = document.getElementById("history");
    const title = history.previousElementSibling;
    const btn = document.getElementById("showMoreBtn");

    if (history.classList.contains("hidden")) {
        history.classList.remove("hidden");
        title.innerText = "Transaction History ↑";

        if (btn && allTransactions.length > visibleCount) {
            btn.classList.remove("hidden");
        }
    } else {
        history.classList.add("hidden");
        title.innerText = "Transaction History ↓";

        if (btn) {
            btn.classList.add("hidden");
        }
    }
}

function loadBalance() {
    const userId = localStorage.getItem("userId");

    fetch(`${baseUrl}/balance?userId=${userId}`, {
        headers: getAuthHeader()
    })
    .then(res => res.json())
    .then(data => {
        document.getElementById("balance").innerText = "Balance: ₹ " + data;
    });
}

let allTransactions = [];
let visibleCount = 10;

function loadTransactions() {
    const userId = localStorage.getItem("userId");

    fetch(`${baseUrl}/transactions?userId=${userId}`, {
        headers: getAuthHeader()
    })
    .then(res => res.json())
    .then(data => {
        allTransactions = data.sort((a, b) => new Date(b.date) - new Date(a.date));

        renderTransactions();
    });
}

function renderTransactions() {
    const list = document.getElementById("history");
    list.innerHTML = "";

    const userName = localStorage.getItem("name");

    allTransactions.slice(0, visibleCount).forEach(t => {
        const li = document.createElement("li");

        const date = new Date(t.date).toLocaleString();
        const type = (t.senderName === userName) ? "Sent" : "Received";

        li.innerText = `${type}: ${t.senderName} → ${t.receiverName} | ₹${t.amount} | ${date}`;

        li.style.color = (type === "Sent") ? "red" : "green";
        li.style.listStyle = "none";
        li.style.padding = "8px";
        li.style.borderBottom = "1px solid #ddd";

        list.appendChild(li);
    });

    const btn = document.getElementById("showMoreBtn");
    if (btn) {  
    if (visibleCount >= allTransactions.length) {
        btn.classList.add("hidden");
    } else {
        btn.classList.remove("hidden");
    }
}
}
function showMore() {
    const history = document.getElementById("history");
    const title = history.previousElementSibling;
    if (history.classList.contains("hidden")) {
        history.classList.remove("hidden");
        title.innerText = "Transaction History ↑";
    }

    visibleCount += 10;
    renderTransactions();
}

function loadPoints() {
    const userId = localStorage.getItem("userId");

    fetch(`${baseUrl}/points?userId=${userId}`, {
        headers: getAuthHeader()
    })
    .then(res => res.json())
    .then(data => {
        document.getElementById("points").innerText = "Points: " + data.points;
    })
    .catch(() => {
        document.getElementById("points").innerText = "Points: Error";
    });
}

function togglePassword(inputId, icon) {
    const input = document.getElementById(inputId);

    if (input.type === "password") {
        input.type = "text";
        icon.innerText = "🙈";
    } else {
        input.type = "password";
        icon.innerText = "👁️";
    }
}
function loadProfile() {
    fetch(`${baseUrl}/profile`, {
        headers: getAuthHeader()
    })
    .then(res => res.json())
    .then(data => {
        document.getElementById("pUserId").innerText = data.userId;
        document.getElementById("pName").innerText = data.name;
        document.getElementById("pEmail").innerText = data.email;
    });
}
function toggleProfile() {
    const profile = document.getElementById("profile");

    profile.classList.toggle("hidden");
    if (!profile.classList.contains("hidden")) {
        loadProfile();
    }
}

function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    window.location.href = "index.html";
}
