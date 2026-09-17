(function () {
    const chatWidget = document.getElementById("chatWidget");
    const chatPopup = document.getElementById("chatPopup");
    const chatToggleBtn = document.getElementById("chatToggleBtn");
    const chatCloseBtn = document.getElementById("chatCloseBtn");
    const chatForm = document.getElementById("chatForm");
    const chatInput = document.getElementById("chatInput");
    const chatMessages = document.getElementById("chatMessages");
    const chatSendBtn = document.getElementById("chatSendBtn");
    const chatError = document.getElementById("chatError");
    const chatProviderOptions = document.getElementById("chatProviderOptions");
    const chatProviderLabel = document.getElementById("chatProviderLabel");
    const chatIcon = chatToggleBtn?.querySelector(".chat-icon");
    const chatIconClose = chatToggleBtn?.querySelector(".chat-icon-close");

    if (!chatWidget || !chatPopup || !chatToggleBtn || !chatForm || !chatInput || !chatMessages) {
        return;
    }

    const welcomeMessage =
        "Hello! I can help you choose healthcare products, explain items in your cart, or answer checkout questions. How can I help you today?";

    const histories = {};
    const providerLabels = {};
    let selectedProvider = "google";
    let isOpen = false;

    function getHistory(provider) {
        if (!histories[provider]) {
            histories[provider] = [];
        }

        return histories[provider];
    }

    function getProviderLabel(provider) {
        return providerLabels[provider] || provider;
    }

    function setPopupOpen(open) {
        isOpen = open;
        chatPopup.classList.toggle("d-none", !open);
        chatPopup.setAttribute("aria-hidden", open ? "false" : "true");
        chatToggleBtn.setAttribute("aria-expanded", open ? "true" : "false");
        chatToggleBtn.setAttribute("aria-label", open ? "Close chat assistant" : "Open chat assistant");
        chatIcon?.classList.toggle("d-none", open);
        chatIconClose?.classList.toggle("d-none", !open);

        if (open) {
            chatInput.focus();
        }
    }

    function appendMessage(text, role) {
        const bubble = document.createElement("div");
        bubble.className = `chat-bubble ${role === "user" ? "user" : "bot"}`;
        bubble.textContent = text;
        chatMessages.appendChild(bubble);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function resetMessages() {
        chatMessages.innerHTML = "";
        appendMessage(welcomeMessage, "bot");
    }

    function showError(message) {
        if (!chatError) {
            return;
        }

        chatError.textContent = message;
        chatError.classList.remove("d-none");
    }

    function hideError() {
        if (!chatError) {
            return;
        }

        chatError.textContent = "";
        chatError.classList.add("d-none");
    }

    function updateProviderUi() {
        const label = getProviderLabel(selectedProvider);
        if (chatProviderLabel) {
            chatProviderLabel.textContent = `Provider: ${label}`;
        }

        chatProviderOptions?.querySelectorAll(".chat-provider-btn").forEach(function (button) {
            const isActive = button.dataset.provider === selectedProvider;
            button.classList.toggle("active", isActive);
            button.setAttribute("aria-pressed", isActive ? "true" : "false");
        });
    }

    function selectProvider(provider) {
        if (!provider || provider === selectedProvider) {
            return;
        }

        selectedProvider = provider;
        updateProviderUi();
        resetMessages();
        hideError();
    }

    function renderProviderButtons(providers) {
        if (!chatProviderOptions || !Array.isArray(providers) || providers.length === 0) {
            return;
        }

        chatProviderOptions.innerHTML = "";
        providers.forEach(function (provider, index) {
            providerLabels[provider.id] = provider.label;

            const button = document.createElement("button");
            button.type = "button";
            button.className = "chat-provider-btn";
            button.dataset.provider = provider.id;
            button.textContent = provider.label;
            button.setAttribute("aria-pressed", "false");

            if (index === 0) {
                selectedProvider = provider.id;
            }

            button.addEventListener("click", function () {
                selectProvider(provider.id);
            });

            chatProviderOptions.appendChild(button);
        });

        updateProviderUi();
    }

    async function loadProviders() {
        try {
            const response = await fetch("/Chatbot/Providers");
            if (!response.ok) {
                return;
            }

            const providers = await response.json();
            renderProviderButtons(providers);
        } catch {
            updateProviderUi();
        }
    }

    chatProviderOptions?.querySelectorAll(".chat-provider-btn").forEach(function (button) {
        providerLabels[button.dataset.provider] = button.textContent?.trim() || button.dataset.provider;
        button.addEventListener("click", function () {
            selectProvider(button.dataset.provider);
        });
    });

    chatToggleBtn.addEventListener("click", function () {
        setPopupOpen(!isOpen);
    });

    chatCloseBtn?.addEventListener("click", function () {
        setPopupOpen(false);
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && isOpen) {
            setPopupOpen(false);
        }
    });

    chatForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const message = chatInput.value.trim();
        if (!message) {
            return;
        }

        const provider = selectedProvider;
        const history = getHistory(provider);

        hideError();
        appendMessage(message, "user");
        chatInput.value = "";
        chatInput.disabled = true;
        chatSendBtn.disabled = true;
        chatProviderOptions?.querySelectorAll(".chat-provider-btn").forEach(function (button) {
            button.disabled = true;
        });

        const typingBubble = document.createElement("div");
        typingBubble.className = "chat-bubble bot typing";
        typingBubble.textContent = provider === "cursor"
            ? "Contacting Cursor agent..."
            : provider === "ollama"
                ? "Asking Ollama..."
                : "Thinking...";
        chatMessages.appendChild(typingBubble);
        chatMessages.scrollTop = chatMessages.scrollHeight;

        try {
            const response = await fetch("/Chatbot/Send", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    message: message,
                    provider: provider,
                    history: history
                })
            });

            const data = await response.json();
            typingBubble.remove();

            if (!data.success) {
                showError(data.error || "Something went wrong. Please try again.");
                return;
            }

            history.push({ role: "user", text: message });
            history.push({ role: "model", text: data.reply });
            appendMessage(data.reply, "bot");
        } catch {
            typingBubble.remove();
            showError("Unable to connect to the chatbot. Please try again.");
        } finally {
            chatInput.disabled = false;
            chatSendBtn.disabled = false;
            chatProviderOptions?.querySelectorAll(".chat-provider-btn").forEach(function (button) {
                button.disabled = false;
            });
            chatInput.focus();
        }
    });

    updateProviderUi();
    loadProviders();
})();
