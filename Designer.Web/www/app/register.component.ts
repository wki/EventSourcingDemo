import { Component } from '@angular/core';
import { Http } from '@angular/http';

enum RegisterState {
    Register,
    Saving,
    Saved,
    Failed,
}

@Component({
    templateUrl: 'templates/register.html',
})
export class RegisterComponent {
    state: RegisterState;
    email: string;
    password: string;

    message: string;

    constructor(private http: Http) {
        this.state = RegisterState.Register;
        this.email = undefined;
        this.password = undefined;

        this.message = '';
    }

    saveRegistration(): void {
        console.log(`save registration email=${this.email}, password=${this.password}`);
        this.state = RegisterState.Saving;

        this.http
            .post(
                'http://localhost:8000/api/registration/save',
                JSON.stringify({email: this.email, password: this.password}))
            .subscribe(
                _ => this.savedRegistration(),
                e => this.failedSaving(e)
            );
    }

    savedRegistration(): void {
        console.log('saved registration');
        this.state = RegisterState.Saved;
    }

    failedSaving(e: any): void {
        console.log('failed saving registration');
        this.state = RegisterState.Failed;
        this.message = e;
    }

    showRegisterForm(): boolean {
        return this.state == RegisterState.Register;
    }

    showSavingMessage(): boolean {
        return this.state == RegisterState.Saving;
    }

    showSavedMessage(): boolean {
        return this.state == RegisterState.Saved;
    }

    showFailedMessage(): boolean {
        return this.state == RegisterState.Failed;
    }

    registerDataInvalid(): boolean {
        if (!this.email || this.email.length < 5)
            return true;
        if (!this.password || this.password.length < 6)
            return true;
        return false;
    }
}
