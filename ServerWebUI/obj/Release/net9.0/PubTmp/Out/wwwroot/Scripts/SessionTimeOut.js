window.sessionTimeout = {
    timer: null,
    warningTimer: null,

    init: function (dotnetHelper, idleMinutes, warningMinutes) {
        const idleTime = idleMinutes * 60 * 1000;
        const warningTime = (idleMinutes - warningMinutes) * 60 * 1000;

        const resetTimer = () => {
            clearTimeout(this.timer);
            clearTimeout(this.warningTimer);

            this.warningTimer = setTimeout(() => {
                if (dotnetHelper) {
                    dotnetHelper.invokeMethodAsync('ShowWarning')
                        .catch(() => console.warn("Blazor circuit lost. Warning skipped."));
                }
            }, warningTime);

            this.timer = setTimeout(() => {
                console.warn("Idle timeout reached. Logging out.");
                if (dotnetHelper) {
                    dotnetHelper.invokeMethodAsync('LogoutUser')
                        .catch(() => {
                            sessionStorage.clear();
                            window.location.href = "/";
                        });
                }
            }, idleTime);
        };

        ['click', 'mousemove', 'keypress', 'scroll', 'touchstart'].forEach(event =>
            document.addEventListener(event, resetTimer, true)
        );

        resetTimer();
        console.log("sessionTimeout.js initialized");
    }
};




