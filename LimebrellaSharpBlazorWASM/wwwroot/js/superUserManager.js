(() => {
    'use strict'

    // SOURCE: canvas-confetti by catdad (https://github.com/catdad/canvas-confetti)
    function randomInRange(min, max) {
        return Math.random() * (max - min) + min;
    }
    function loadConfettiLibrary(callback) {
        var script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/canvas-confetti@1.9.3/dist/confetti.browser.min.js';
        script.onload = callback;
        document.head.appendChild(script);
    }
    function fireConfetti() {
        confetti({
            colors: ['244709', '458018', '8BB06F', '89F336'],
            angle: randomInRange(55, 125),
            spread: randomInRange(50, 70),
            particleCount: randomInRange(50, 100),
            origin: { y: 0.6 }
        });
    }
    // END OF SOURCE

    function fireConfettiXTimes(x) {
        // Load the confetti library and then assign the function to the window object
        loadConfettiLibrary(() => {
            for (let i = 0; i < x; i++) { setTimeout(fireConfetti, i * 280); }
        });
    }

    // CONSTANTS
    const GOAL = 3
    // VARIABLES
    var COUNTER = 0
    var IS_BUSY = false

    window.handleSuperUserActivationClick = () => {
        if (!IS_BUSY) {
            // set timer
            setTimeout(() => { COUNTER = 0; IS_BUSY = false; }, 450)
            IS_BUSY = true
        }
        COUNTER++
        const test = COUNTER === GOAL;
        if (test) {
            fireConfettiXTimes(3)
            console.log("You're a Superuser now!")
        }
        return (test)
    }
})()