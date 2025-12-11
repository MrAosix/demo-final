let ws = new WebSocket(`ws://${location.hostname}:8081/ws`);

let inGame = false;
let isReady = false;
let myId = null;
let myPlayer = null;

// Panels
let joinPanel = document.getElementById("joinPanel");
let readyPanel = document.getElementById("readyPanel");
let countdownPanel = document.getElementById("countdownPanel");
let controllerPanel = document.getElementById("controllerPanel");

// UI elements
let nameDisplay = document.getElementById("nameDisplay");
let seatDisplay = document.getElementById("seatDisplay");
let colorDisplay = document.getElementById("colorDisplay");
let readyBtn = document.getElementById("readyBtn");

// Buttons
let ctrlButtons = document.querySelectorAll(".ctrl");

    let lastTouch = 0;
    document.addEventListener('touchend', function (e) {
        const now = new Date().getTime();
        if (now - lastTouch < 300) {
            e.preventDefault();
        }
        lastTouch = now;
    }, { passive: false });

ws.onopen = () => {
    console.log("Connected to WS");
    ws.send(JSON.stringify({ action: "join" }));
};

ws.onmessage = (event) => {
    let data = JSON.parse(event.data);

    if (data.type === "id") {
        myId = data.id;
        return;
    }

    if (data.type === "waiting") {
        joinPanel.innerHTML = "<h2>Waiting for game to finish…</h2>";
        return;
    }

    if (data.type === "reset") {
        console.log("RESET RECEIVED");

        inGame = false;
        isReady = false;
        myPlayer = null;

        joinPanel.style.display = "block";
        readyPanel.style.display = "none";
        countdownPanel.style.display = "none";
        controllerPanel.style.display = "none";

        joinPanel.innerHTML = "<h2>Connecting...</h2>";
        ws.send(JSON.stringify({ action: "join" }));
        return;
    }

    if (data.type === "state") {
        updateUI(data.players);
    }
};

function updateUI(players) {
    if (inGame) return;
    if (!myId) return;

    let me = players.find(p => p.id === myId);
    if (!me) return;

    myPlayer = me;

    joinPanel.style.display = "none";
    readyPanel.style.display = "block";
    nameDisplay.textContent = me.name;
    seatDisplay.textContent = me.seat;
    colorDisplay.style.background = me.color;

    if (me.ready && !isReady) {
        isReady = true;
        readyBtn.classList.add("ready");
        readyBtn.textContent = "READY ✔";
        readyBtn.disabled = true;
    }

    if (players.length > 0 && players.every(p => p.ready)) {
        startCountdown();
    }
}

readyBtn.onclick = () => {
    readyBtn.classList.add("ready");
    readyBtn.textContent = "READY ✔";
    readyBtn.disabled = true;

    ws.send(JSON.stringify({ action: "ready" }));
};

function startCountdown() {
    if (inGame) return;

    inGame = true;

    readyPanel.style.display = "none";
    countdownPanel.style.display = "block";

    let cd = 3;
    countdownPanel.innerHTML = `<h2>${cd}</h2>`;

    let interval = setInterval(() => {
        cd--;
        if (cd > 0) {
            countdownPanel.innerHTML = `<h2>${cd}</h2>`;
        } else {
            clearInterval(interval);
            countdownPanel.style.display = "none";
            controllerPanel.style.display = "flex";
        }
    }, 1000);
}

function sendInput(dir) {
    ws.send(JSON.stringify({
        action: "input",
        value: dir
    }));
}

ctrlButtons.forEach(btn => {
    btn.onclick = () => sendInput(btn.dataset.dir);
});


