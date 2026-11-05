window.kdsAudio = (() => {
    let audioContext;

    async function ativar() {
        const AudioContext =
            window.AudioContext || window.webkitAudioContext;

        if (!AudioContext) {
            throw new Error("Áudio não suportado.");
        }

        if (!audioContext) {
            audioContext = new AudioContext();
        }

        if (audioContext.state === "suspended") {
            await audioContext.resume();
        }
    }

    function tocar() {
        if (!audioContext || audioContext.state !== "running") {
            return;
        }

        const oscillator = audioContext.createOscillator();
        const gain = audioContext.createGain();

        oscillator.connect(gain);
        gain.connect(audioContext.destination);

        oscillator.frequency.value = 880;
        oscillator.type = "sine";

        gain.gain.setValueAtTime(0.3, audioContext.currentTime);
        gain.gain.exponentialRampToValueAtTime(
            0.01,
            audioContext.currentTime + 0.4
        );

        oscillator.start();
        oscillator.stop(audioContext.currentTime + 0.4);
    }

    return { ativar, tocar };
})();