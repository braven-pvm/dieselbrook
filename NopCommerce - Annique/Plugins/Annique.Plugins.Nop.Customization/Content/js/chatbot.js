$(document).ready(function () {

    $(document).on('click', '.buy-now-btn', async function () {
        const $button = $(this); // Cache the clicked button
        const originalText = $button.html(); // Store original content
        const productId = $(this).data('product-id');
        const quantity = $(this).data('quantity') || 1;
        const cartType = $(this).data('cart-type') || 'ShoppingCart';

        // Disable button and show loading state
        $button.prop('disabled', true);
        $button.html('Adding...');

        try {
            const response = await fetch(`/AddRecommendProductToCart?productId=${productId}&shoppingCartType=${cartType}&quantity=${quantity}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            const result = await response.json();

            if (!response.ok) {
                displayBarNotification("Failed to add product to cart.", 'error', 0);
                return;
            }

            if (result.Success) {
                displayBarNotification(result.Message, 'success', 3500);
            } else if (result.Errors && result.Errors.length > 0) {
                displayBarNotification(result.Errors.join('\n'), 'error', 0);
            } else {
                displayBarNotification(result.Message || "Unable to add product to cart", 'error', 0);
            }
        }
        catch (error) {
            displayBarNotification("An error occurred while adding to cart.", 'error', 0);
        }
        finally {
            // Re-enable button and restore original text
            $button.prop('disabled', false);
            $button.html(originalText);
        }
    });

});

function toggleChatbot() {
    const box = document.getElementById('chatbot-box');
    box.classList.toggle('active');
}

function sendMessage() {
    const input = document.getElementById("chatbot-input");
    const sendBtn = document.getElementById("chatbot-send");
    const msg = input.value.trim();

    if (!msg) return;

    // Disable input and button
    input.disabled = true;
    sendBtn.disabled = true;
    sendBtn.textContent = "Sending...";

    appendMessage("You", msg);
    input.value = "";

    fetch("/GetRecommendation", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message: msg })
    })
    .then(res => res.json())
    .then(data => {
        const botReply = data.reply || "Sorry, I didn't get that.";
        appendMessage("Bot annalie", botReply, msg);
    })
    .catch(() => {
        appendMessage("Bot annalie", "⚠️ There was a problem getting a response. Please try again later.");
    })
    .finally(() => {
        // Re-enable input and button
        input.disabled = false;
        sendBtn.disabled = false;
        sendBtn.textContent = "Send";
        input.focus();
    });
}

function appendMessage(sender, text, originalUserMessage = "") {
    const container = document.getElementById("chatbot-messages");
    const msg = document.createElement("div");
    msg.style.marginBottom = "15px";

    let content = `<strong>${sender}:</strong> ${text}`;

    if (sender === "Bot annalie") {
        const feedbackKey = Date.now(); // unique per message

        content += `
                <div style="margin-top: 10px; font-size: 14px;" id="feedback-${feedbackKey}">
                    Was this helpful?
                    <button
                        data-status="helpful"
                        data-original="${encodeURIComponent(originalUserMessage)}"
                        data-response="${encodeURIComponent(text)}"
                        data-key="${feedbackKey}"
                        onclick="submitFeedbackFromEvent(this)">👍 Yes</button>
                    <button
                        data-status="not_helpful"
                        data-original="${encodeURIComponent(originalUserMessage)}"
                        data-response="${encodeURIComponent(text)}"
                        data-key="${feedbackKey}"
                        onclick="submitFeedbackFromEvent(this)">👎 No</button>
                </div>
            `;
    }

    msg.innerHTML = content;
    container.appendChild(msg);
    container.scrollTop = container.scrollHeight;
}

function submitFeedbackFromEvent(btn) {
    const status = btn.dataset.status;
    const originalMessage = decodeURIComponent(btn.dataset.original || "");
    const aiResponse = decodeURIComponent(btn.dataset.response || "");
    const feedbackKey = btn.dataset.key;
    const feedbackDiv = document.getElementById(`feedback-${feedbackKey}`);

    if (!status || !originalMessage) return;

    feedbackDiv.innerHTML = "Submitting feedback...";

    fetch("/SubmitChatFeedback", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            originalMessage,
            aiResponse,
            status
        })
    })
    .then(res => {
        if (!res.ok) throw new Error("Server error");
        return res.json();
    })
    .then(data => {
        if (data.success) {
            feedbackDiv.innerHTML = `<span style="color: green;">Thanks for your feedback!</span>`;
        } else {
            feedbackDiv.innerHTML = `<span style="color: orange;">Feedback not saved.</span>`;
        }
    })
    .catch(() => {
        feedbackDiv.innerHTML = `<span style="color: red;">Error submitting feedback. Please try again later.</span>`;
    });
}