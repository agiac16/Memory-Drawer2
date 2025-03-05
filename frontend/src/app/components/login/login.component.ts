import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; 

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class LoginComponent {
  username: string = '';
  password: string = '';
  apiUrl = 'http://localhost:5000/api/users/login';
  errorMessage :string = ""


  constructor(private http: HttpClient, private router: Router) { }

  async login() { 
    const payload = { username: this.username, password: this.password };

    try {
      const response = await firstValueFrom(this.http.post<{ success: boolean, token?: string, userId?: string, message?: string }>(this.apiUrl, payload));
      if (response.success) {
        // Handle successful login, e.g., store the token
        console.log('Login successful:', response.token);

        this.router.navigate(['/']); // Redirect to HomePage

        if (response.token) {
          localStorage.setItem('token', response.token);
        } else {
          console.error('Token is undefined');
        }

        if (response.userId) {
          localStorage.setItem('userId', response.userId);
        } else {
          console.error('User ID is undefined');
        }

        this.router.navigate(['/dashboard']);
      } else {
        // Handle login failure
        console.error('Login failed:', response.message);
        this.errorMessage = response.message || 'Invalid credentials. Please try again.';

      }
    } catch (error) {
      // Handle server error
      console.error('Server error:', error);
      this.errorMessage = 'Unable to connect to the server. Please try again later.';

    }
  }
}