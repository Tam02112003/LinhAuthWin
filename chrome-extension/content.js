(function () {
  const KEYWORD_RE = /otp|one[-\s]?time[-\s]?code|verification[-\s]?code|auth(entication)?[-\s]?code|security[-\s]?code|passcode|2fa|m[aã]\s*(x[aá]c\s*th[uự]c|otp|b[aả]o\s*m[aậ]t)/i;
  const TRIGGER_COOLDOWN_MS = 15000;

  let lastTriggerAt = 0;

  function nearbyText(input) {
    let node = input;
    let text = (input.placeholder || "") + " " + (input.getAttribute("aria-label") || "") +
      " " + (input.name || "") + " " + (input.id || "") + " " + (input.autocomplete || "");

    const labelledBy = input.getAttribute("aria-labelledby");
    if (labelledBy) {
      const labelEl = document.getElementById(labelledBy);
      if (labelEl) text += " " + labelEl.textContent;
    }
    if (input.id) {
      const label = document.querySelector(`label[for="${CSS.escape(input.id)}"]`);
      if (label) text += " " + label.textContent;
    }

    for (let depth = 0; depth < 3 && node; depth++) {
      node = node.parentElement;
      if (node) text += " " + node.textContent.slice(0, 200);
    }
    return text;
  }

  function looksLikeOtpInput(input) {
    if (input.autocomplete === "one-time-code") return true;
    if (!/^(text|tel|number|password)$/i.test(input.type || "text")) return false;
    return KEYWORD_RE.test(nearbyText(input));
  }

  function hasSingleDigitInputGroup() {
    const candidates = Array.from(document.querySelectorAll('input[maxlength="1"]'));
    if (candidates.length < 4) return false;
    return candidates.some((el) => el.offsetParent !== null);
  }

  function scanForOtpField() {
    const inputs = document.querySelectorAll('input[type="text"], input[type="tel"], input[type="number"], input[type="password"], input:not([type])');
    for (const input of inputs) {
      if (input.offsetParent === null) continue; // skip hidden inputs
      if (looksLikeOtpInput(input)) return true;
    }
    return hasSingleDigitInputGroup();
  }

  function maybeTrigger() {
    const now = Date.now();
    if (now - lastTriggerAt < TRIGGER_COOLDOWN_MS) return;
    if (!scanForOtpField()) return;

    lastTriggerAt = now;
    chrome.runtime.sendMessage({ type: "otp-detected" });
  }

  maybeTrigger();

  const observer = new MutationObserver(() => maybeTrigger());
  observer.observe(document.documentElement, { childList: true, subtree: true });
})();
