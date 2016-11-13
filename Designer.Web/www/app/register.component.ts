import { Component } from '@angular/core';
import { Headers, Http } from '@angular/http';

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
    fullname: string;

    message: string;

    constructor(private http: Http) {
        this.state = RegisterState.Register;
        this.email = undefined;
        this.fullname = undefined;

        this.message = '';
    }

    saveRegistration(): void {
        console.log(`save registration email=${this.email}, fullname=${this.fullname}`);
        this.state = RegisterState.Saving;

        var headers = new Headers();
        headers.append("Content-Type", 'application/json');

        this.http
            .post(
                'http://localhost:9000/api/person/register',
                JSON.stringify({email: this.email, fullname: this.fullname}),
                {headers: headers})
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
        if (!this.fullname || this.fullname.length < 3)
            return true;
        return false;
    }
}
