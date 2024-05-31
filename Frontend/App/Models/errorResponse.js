export class ErrorResponse {
    constructor(message) {
        this.message = message;
    }

    drawResponse(container) {
        const errorDiv = document.createElement("div");
        errorDiv.classList.add("error-response");

        const errorMessage = document.createElement("label");
        errorMessage.classList.add("error-message");
        errorMessage.textContent = `${this.message}`;

        errorDiv.appendChild(errorMessage);
        container.appendChild(errorDiv);
    }
}