let ws = new WebSocket("ws://192.168.8.172:8750/ws");

/* ============================================================
   ORIENTATION LOCK (SHOW OVERLAY IF PORTRAIT)
============================================================ */
const rotateOverlay = document.getElementById("rotateOverlay");

function checkOrientation() {
    const portrait = window.innerHeight > window.innerWidth;

    if (portrait) {
        rotateOverlay.style.display = "flex";
        document.body.style.overflow = "hidden"; // block interaction
    } else {
        rotateOverlay.style.display = "none";
        document.body.style.overflow = "auto";
    }
}

window.addEventListener("resize", checkOrientation);
window.addEventListener("orientationchange", checkOrientation);

checkOrientation(); // run immediately

/* ============================================================
   WEBSOCKET SETUP
============================================================ */


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


/* ============================================================
   WEBSOCKET EVENTS
============================================================ */
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

        joinPanel.innerHTML = "<h2>Connection...</h2>";
        ws.send(JSON.stringify({ action: "join" }));
        return;
    }

    if (data.type === "state") {
        updateUI(data.players);
    }
};


/* ============================================================
   UI UPDATE
============================================================ */
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
        readyBtn.textContent = "PRÊT ✔";
        readyBtn.disabled = true;
    }

    if (players.length > 0 && players.every(p => p.ready)) {
        startCountdown();
    }
}


/* ============================================================
   READY BUTTON
============================================================ */
readyBtn.addEventListener("click", () => {
    readyBtn.classList.add("ready");
    readyBtn.textContent = "PRÊT ✔";
    readyBtn.disabled = true;

    ws.send(JSON.stringify({ action: "ready" }));
});


/* ============================================================
   COUNTDOWN
============================================================ */
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


/* ============================================================
   ARROW INPUT (NO FEEDBACK)
============================================================ */
function sendInput(dir) {
    ws.send(JSON.stringify({
        action: "input",
        value: dir
    }));
}

ctrlButtons.forEach(btn => {
    btn.addEventListener("touchstart", () => {
        sendInput(btn.dataset.dir);
    }, { passive: true });

    btn.addEventListener("click", () => {
        sendInput(btn.dataset.dir);
    });
});
