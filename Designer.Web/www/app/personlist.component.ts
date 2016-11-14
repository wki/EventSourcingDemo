import { Component } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Person } from './dto/person';

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