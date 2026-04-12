(function() {
    const cards = document.querySelectorAll(".card");

    cards.forEach((card) => {
        card.addEventListener("mousemove", handleMouseMove);
    });

    function handleMouseMove(e) {
        const rect = this.getBoundingClientRect();
        const mouseX = e.clientX - rect.left - rect.width / 2;
        const mouseY = e.clientY - rect.top - rect.height / 2;

        let angle = Math.atan2(mouseY, mouseX) * (180 / Math.PI);

        angle = (angle + 360) % 360;

        this.style.setProperty("--start", angle + 60);

        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        if(this.classList.contains("card-editor")) {
            this.style.setProperty("--mouse-x", `${x}px`);
            this.style.setProperty("--mouse-y", `${y}px`);
        }

        if(this.classList.contains("card-gradient")) {
            this.style.setProperty("--mouse-g-x", `${x}px`);
            this.style.setProperty("--mouse-g-y", `${y}px`);
        }
    }
})();