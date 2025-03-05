import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; // Import Router

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
  errorMessage: string = ''; // Store error message
  apiUrl = 'http://localhost:5000/api/user/signup';

  constructor(private http: HttpClient, private router: Router) {} // Inject Router

  async signup() { 
    this.errorMessage = '';
    const payload = { username: this.username, email: this.email, password: this.password };
    
    try {
      const response = await firstValueFrom(
        this.http.post<{ success: boolean; message?: string; token?: string }>(
          this.apiUrl, payload
        )
      );

      if (response.success) {
        console.log('Signup successful', response);
        if (response.token) {
          localStorage.setItem('token', response.token); // Store token
        }

        this.router.navigate(['/']);
        
      } else {
        this.errorMessage = response.message || 'Signup failed. Please try again.';
      }
    } catch (error: any) {
      console.error('Signup Error:', error);

      // Handle different error messages
      if (error.error?.message) {
        this.errorMessage = error.error.message;
      } else {
        this.errorMessage = 'An error occurred during signup. Please try again later.';
      }
    }
  }
}