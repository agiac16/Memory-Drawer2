import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  isLoggedIn: boolean = false;

  ngOnInit(): void {
    this.checkLoginStatus(); // ✅ Ensures isLoggedIn is updated on component load
  }

  checkLoginStatus() {
    this.isLoggedIn = !!localStorage.getItem('token'); // ✅ Convert to boolean
  }

  login() {
    localStorage.setItem('token', 'user-token'); // Simulate login
    this.checkLoginStatus(); // Update button state
  }

  logout() {
    localStorage.removeItem('token'); // Remove token
    this.checkLoginStatus(); // Update button state
  }
}