import { Component, inject } from '@angular/core';

import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.css'
})
export class AppComponent {
  title = 'korp-frontend';

  http = inject(HttpClient);
  public produtos: any[] = [];
  public isLoading = false;
  public displayedColumns: string[] = ['codigo', 'descricao', 'saldo'];

  fetchProdutos() {
    this.isLoading = true;

    this.http.get<any[]>(`${environment.apiEstoqueUrl}/produtos`)
      .subscribe(data => {
        this.produtos = data;
        this.isLoading = false;
      }, error => {
        console.error('Erro ao buscar produtos:', error);
        alert('Erro ao buscar produtos. Verifique o console.');
        this.isLoading = false;
      });
  }
}