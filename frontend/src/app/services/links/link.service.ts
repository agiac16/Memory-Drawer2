import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Link } from '../../models/link.model';

@Injectable({
  providedIn: 'root'
})
export class LinkService {
  private apiUrl = 'http://localhost:5000/api/link'; 

  constructor(private http: HttpClient) { }

  getUserLinks(userId: string): Observable<{ success: boolean; data: Link[] }> {
    return this.http.get<{ success: boolean; data: Link[] }>(`${this.apiUrl}/all`, { params: { userId } });
  }
}
