import { Component } from '@angular/core';
import { Http, Response } from '@angular/http';

class Person {
    constructor(public id:number, public fullname:string, public email:string) { }
}

@Component({
    templateUrl: 'templates/personlist.html',
})
export class PersonListComponent {
    persons: Person[];

    constructor(private http: Http) {
        this.loadPersons();
    }

    loadPersons(): void {
        this.http.get("http://localhost:9000/api/person/list")
            .subscribe(r => this.persons = <Person[]> r.json());
    }
}