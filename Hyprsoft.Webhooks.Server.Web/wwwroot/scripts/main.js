var UptimeManager = /** @class */ (function () {
    function UptimeManager(days, hours, minutes, seconds) {
        if (days === void 0) { days = 0; }
        if (hours === void 0) { hours = 0; }
        if (minutes === void 0) { minutes = 0; }
        if (seconds === void 0) { seconds = 0; }
        this.days = days;
        this.hours = hours;
        this.minutes = minutes;
        this.seconds = seconds;
    }
    UptimeManager.prototype.getUptime = function () {
        if (this.seconds >= 59) {
            this.seconds = 0;
            if (this.minutes >= 59) {
                this.minutes = 0;
                if (this.hours >= 23) {
                    this.hours = 0;
                    this.days++;
                }
                else
                    this.hours++;
            }
            else
                this.minutes++;
        }
        else
            this.seconds++;
        return this.days + " " + (this.days === 1 ? 'day' : 'days') + " " + (this.hours < 10 ? '0' + this.hours : this.hours) + ":" + (this.minutes < 10 ? '0' + this.minutes : this.minutes) + ":" + (this.seconds < 10 ? '0' + this.seconds : this.seconds);
    };
    return UptimeManager;
}());
//# sourceMappingURL=main.js.map