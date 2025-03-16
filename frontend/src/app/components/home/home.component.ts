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
    this.checkLoginStatus(); 
  }

  checkLoginStatus() {
    this.isLoggedIn = !!localStorage.getItem('token'); 
  }

  login() {
    localStorage.setItem('token', 'user-token'); // Simulate login
    this.checkLoginStatus(); // Update button state
  }

  logout() {
    localStorage.removeItem('token'); // Remove token
    localStorage.removeItem('userId');
    this.checkLoginStatus(); // Update button state
  }
}