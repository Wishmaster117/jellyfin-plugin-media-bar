/* ALEXFLIX_RC6A_DEVICE_LOADER_V38_SAFE_UI_V41 */
(function () {
    "use strict";

    var STORAGE_KEY = "alexflix.device.disabled.v1";
    var RESTORE_ID = "alexflix-device-restore";

    var translations = {
        fr: {
            disabled: "MediaBar Désactivée",
            enable: "Afficher MediaBar sur cet appareil",
            restore: "Réactiver MediaBar sur cet appareil"
        },
        en: {
            disabled: "MediaBar Disabled",
            enable: "Enable MediaBar on this device",
            restore: "Enable MediaBar on this device"
        },
        de: {
            disabled: "MediaBar deaktiviert",
            enable: "MediaBar auf diesem Gerät aktivieren",
            restore: "MediaBar auf diesem Gerät wieder aktivieren"
        },
        es: {
            disabled: "MediaBar desactivada",
            enable: "Activar MediaBar en este dispositivo",
            restore: "Volver a activar MediaBar"
        },
        pt: {
            disabled: "MediaBar desativada",
            enable: "Ativar MediaBar neste dispositivo",
            restore: "Reativar MediaBar neste dispositivo"
        },
        it: {
            disabled: "MediaBar disattivata",
            enable: "Attiva MediaBar su questo dispositivo",
            restore: "Riattiva MediaBar su questo dispositivo"
        }
    };

    function language() {
        var raw = document.documentElement.lang ||
            navigator.language ||
            navigator.userLanguage ||
            "en";

        var code = String(raw).toLowerCase().split(/[-_]/)[0];
        return translations[code] ? code : "en";
    }

    function translate(key) {
        var lang = language();
        return translations[lang][key] || translations.en[key];
    }

    function isDisabled() {
        try {
            return localStorage.getItem(STORAGE_KEY) === "1";
        } catch (e) {
            return false;
        }
    }

    function setEnabled(enabled) {
        try {
            if (enabled) {
                localStorage.removeItem(STORAGE_KEY);
            } else {
                localStorage.setItem(STORAGE_KEY, "1");
            }

            if (isDisabled() === !enabled) {
                window.location.reload();
                return true;
            }
        } catch (e) {
            console.error("[ALEXFLIX] Cannot save device preference", e);
        }

        window.alert("MediaBar: unable to save the device preference.");
        return false;
    }

    window.MediaBarAlexflixDevice = {
        isEnabled: function () { return !isDisabled(); },
        setEnabled: setEnabled,
        translate: translate
    };

    var loaderTag = document.currentScript;
    var assetBase = loaderTag && loaderTag.src ?
        loaderTag.src.slice(0, loaderTag.src.lastIndexOf("/") + 1) :
        "";

    function loadMediaBar() {
        if (!assetBase) {
            console.error("[ALEXFLIX] Asset base URL unavailable");
            return;
        }

        var css = document.createElement("link");
        css.rel = "stylesheet";
        css.href = assetBase + "slideshowpure.css";
        document.head.appendChild(css);

        var js = document.createElement("script");
        js.src = assetBase + "slideshowpure.js";
        js.async = false;
        js.onerror = function () {
            console.error("[ALEXFLIX] Main script loading failed");
        };
        document.head.appendChild(js);
    }

    function ensureRestoreButton() {
        if (!isDisabled()) {
            return;
        }

        var candidates = document.querySelectorAll(".homeSectionsContainer");
        var rows = null;

        for (var i = 0; i < candidates.length; i++) {
            var candidate = candidates[i];
            var owner = candidate.closest ? candidate.closest(".page") : null;

            if (!owner || !owner.classList.contains("hide")) {
                rows = candidate;
                break;
            }
        }

        var button = document.getElementById(RESTORE_ID);

        if (!rows || !rows.parentNode) {
            if (button) {
                button.style.display = "none";
            }
            return;
        }

        if (!button) {
            button = document.createElement("button");
            button.id = RESTORE_ID;
            button.type = "button";
            button.style.cssText =
                "display:block;" +
                "width:max-content;" +
                "max-width:calc(100% - 2rem);" +
                "margin:0.65rem 1rem 1rem;" +
                "padding:0.65rem 1rem;" +
                "border:1px solid rgba(255,255,255,0.35);" +
                "border-radius:0.6rem;" +
                "background:rgba(35,35,40,0.90);" +
                "color:#fff;" +
                "font:inherit;" +
                "cursor:pointer;" +
                "outline-offset:3px;";

            button.addEventListener("click", function () {
                setEnabled(true);
            });
        }

        var label = translate("disabled");
        var hint = translate("restore");

        if (button.textContent !== label) {
            button.textContent = label;
        }
        if (button.title !== hint) {
            button.title = hint;
        }
        if (button.getAttribute("aria-label") !== hint) {
            button.setAttribute("aria-label", hint);
        }

        var page = rows.closest ? rows.closest(".page") : null;
        var hidden = page && page.classList.contains("hide");

        button.style.display = hidden ? "none" : "block";

        if (button.parentNode !== rows.parentNode ||
            button.nextSibling !== rows) {
            rows.parentNode.insertBefore(button, rows);
        }
    }

    function initRestoreUI() {
        var pending = false;

        function schedule() {
            if (pending) {
                return;
            }

            pending = true;

            window.requestAnimationFrame(function () {
                pending = false;
                ensureRestoreButton();
            });
        }

        ensureRestoreButton();

        if (typeof MutationObserver === "function") {
            var observer = new MutationObserver(schedule);
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        } else {
            window.setInterval(schedule, 2000);
        }

        window.addEventListener("hashchange", schedule);
        window.addEventListener("pageshow", schedule);

        if (typeof MutationObserver === "function") {
            var languageObserver = new MutationObserver(schedule);
            languageObserver.observe(document.documentElement, {
                attributes: true,
                attributeFilter: ["lang"]
            });
        }
    }

    function checkRecoveryUrl() {
        try {
            var url = new URL(window.location.href);

            if (url.searchParams.get("alexflix") !== "restore") {
                return false;
            }

            url.searchParams.delete("alexflix");

            if (window.history &&
                typeof window.history.replaceState === "function") {
                window.history.replaceState(
                    window.history.state,
                    "",
                    url.pathname + url.search + url.hash
                );
            }

            if (!isDisabled()) {
                return false;
            }

            if (typeof window.confirm !== "function") {
                return false;
            }

            if (window.confirm(translate("restore") + "?")) {
                return setEnabled(true);
            }
        } catch (e) {
            console.warn("[ALEXFLIX] Recovery URL unavailable", e);
        }

        return false;
    }

    if (isDisabled()) {
        if (checkRecoveryUrl()) {
            return;
        }
        if (document.readyState === "loading") {
            document.addEventListener(
                "DOMContentLoaded",
                initRestoreUI,
                { once: true }
            );
        } else {
            initRestoreUI();
        }
    } else {
        loadMediaBar();
    }
})();
