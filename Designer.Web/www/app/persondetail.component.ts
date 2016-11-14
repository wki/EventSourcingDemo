import { Component } from '@angular/core';
import { Http, Response } from '@angular/http';
import { ActivatedRoute } from '@angular/router';
import { Person } from './dto/person';

@Component({
    templateUrl: 'templates/persondetail.html'
})
export class PersonDetailComponent {
    person: Person;

    constructor(private http: Http, private route: ActivatedRoute) {
        let id = route.snapshot.params['id'];
        this.loadPerson(id);
    }

    loadPerson(id: any): void {
        this.http.get(`http://localhost:9000/api/person/${id}`)
            .subscribe(r => this.person = <Person> r.json());
    }
}