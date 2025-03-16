import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent {
  username: string = '';
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  apiUrl = 'http://localhost:5000/api/user/signup';

  constructor(private http: HttpClient, private router: Router) {}

  async signup() { 
    this.errorMessage = '';
    const payload = { username: this.username, email: this.email, password: this.password };

    try {
      const response = await firstValueFrom(
        this.http.post<{ success: boolean; message?: string; token?: string, userId?: string }>(this.apiUrl, payload)
      );

      if (response.success) {
        console.log('Signup successful', response);

        // Store the token for auto-login
        if (response.token) {
          localStorage.setItem('token', response.token);
        }
        if (response.userId) { 
          localStorage.setItem('userId', response.userId)
        }

        // Redirect user after signup
        this.router.navigate(['/dashboard']);
      } else {
        this.errorMessage = response.message || 'Signup failed. Please try again.';
      }
    } catch (error: any) {
      console.error('Signup Error:', error);
      this.errorMessage = error.error?.message || 'An error occurred during signup. Please try again later.';
    }
  }
}