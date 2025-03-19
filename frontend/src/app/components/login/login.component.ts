import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = ''; 
  password: string = ''; 
  apiUrl = 'http://localhost:5000/api/user/login';
  errorMessage: string = '';

  constructor(private http: HttpClient, private router: Router) {}

  async login() { 
    this.errorMessage = '';  // Clear previous errors
  
    // Ensure valid payload is sent
    const payload = { 
      email: this.email.trim(), // Ensure you're sending the correct field (email)
      password: this.password.trim() 
    };
  
    try {
      const response = await firstValueFrom(
        this.http.post<{ success: boolean, token?: string, userId?: string, message?: string }>(
          this.apiUrl, payload
        )
      );
  
      if (response.success) {  
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
  
        this.router.navigate(['/dashboard']); // Redirect to dashboard
      } else {
        console.error('Login failed:', response.message);
        this.errorMessage = response.message || 'Invalid credentials. Please try again.';
      }
    } catch (error: any) {
      console.error('Server error:', error);
  
      // Handle validation errors returned from the backend
      if (error.status === 400 && error.error?.errors) {
        const validationErrors = Object.values(error.error.errors).flat();
        this.errorMessage = validationErrors.join(' '); // Display all validation errors
      } else if (error.error?.message) {
        this.errorMessage = error.error.message;
      } else {
        this.errorMessage = 'An error occurred. Please try again.';
      }
    }
  }
}