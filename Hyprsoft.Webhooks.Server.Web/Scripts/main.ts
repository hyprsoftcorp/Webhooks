class UptimeManager {

    constructor(public days: number = 0, public hours: number = 0, public minutes: number = 0, public seconds: number = 0) {
    }

    getUptime(): string {
        if (this.seconds >= 59) {
            this.seconds = 0;
            if (this.minutes >= 59) {
                this.minutes = 0
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

        return `${this.days} ${this.days === 1 ? 'day' : 'days'} ${this.hours < 10 ? '0' + this.hours : this.hours}:${this.hours < 10 ? '0' + this.minutes: this.minutes}:${this.seconds < 10 ? '0' + this.seconds: this.seconds}`
    }
}