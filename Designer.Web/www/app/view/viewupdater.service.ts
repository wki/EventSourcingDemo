import { Injectable } from '@angular/core';

/** Handle updating views 
 * @class ViwUpdaterService
 * @description change views based on changes reported by website
 */
@Injectable()
export class ViewUpdaterService {
    // wir müssen speichern:
    // - Komponenten und deren View mit URL
    // - signalR Link

    constructor() {
    }

    /** mark a view as visible
     * @name showView
     * @argument view: any
     * @argument name: string
     */
    public showView(view: any, name:string): void {
        console.log(`show ${name}`);
    }

    /** mark a view as invisible
     * @name hideView
     * @argument view: any
     */
    public hideView(view: any): void {
        console.log(`hide ${view}`);
    }
}