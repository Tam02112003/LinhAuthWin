const AUTHWIN_SHOW_URL = "http://127.0.0.1:58712/show";

chrome.runtime.onMessage.addListener((message) => {
  if (message && message.type === "otp-detected") {
    fetch(AUTHWIN_SHOW_URL, { method: "GET", cache: "no-store" }).catch(() => {
      // AuthWin isn't running; nothing to do.
    });
  }
});
