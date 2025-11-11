import { Component, inject } from '@angular/core';

import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { v4 as uuidv4 } from 'uuid';
import { MatListModule } from '@angular/material/list';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { HttpHeaders } from '@angular/common/http';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatListModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.css',
})
export class AppComponent {
  title = 'korp-frontend';

  http = inject(HttpClient);
  public produtos: any[] = [];
  public isLoadingProdutos = false;
  public displayedColumns: string[] = ['codigo', 'descricao', 'saldo'];
  public notasFiscais: any[] = [];
  public isLoadingNotas = false;

  snackBar = inject(MatSnackBar);

  fetchProdutos() {
    this.isLoadingProdutos = true;

    this.http.get<any[]>(`${environment.apiEstoqueUrl}/produtos`).subscribe(
      (data) => {
        this.produtos = data;
        this.isLoadingProdutos = false;
      },
      (error) => {
        console.error('Erro ao buscar produtos:', error);
        const errorMsg = error.error?.message || 'Erro desconhecido';
        this.snackBar.open(`Erro ao buscar produtos: ${errorMsg}`, 'Fechar', {
          duration: 4500,
          panelClass: ['snackbar-error'],
        });
        this.isLoadingProdutos = false;
      },
    );
  }

  fetchNotasFiscais() {
    this.isLoadingNotas = true;

    this.http.get<any[]>(`${environment.apiFaturamentoUrl}/notasfiscais`).subscribe(
      (data) => {
        this.notasFiscais = data;
        this.isLoadingNotas = false;
      },
      (error) => {
        console.error('Erro ao buscar notas:', error);
        const errorMsg = error.error?.message || 'Erro desconhecido';
        this.snackBar.open(`Erro ao buscar as notas: ${errorMsg}`, 'Fechar', {
          duration: 4500,
          panelClass: ['snackbar-error'],
        });
        this.isLoadingNotas = false;
      },
    );
  }

  imprimirNota(notaId: string) {
    const idempotencyKey = uuidv4();
    const headers = new HttpHeaders({
      'X-Idempotency-Key': idempotencyKey,
    });

    this.snackBar.open(`Enviada para impressão...`, 'OK', {
      duration: 3000,
    });

    this.http
      .post(`${environment.apiFaturamentoUrl}/notasfiscais/${notaId}/imprimir`, null, {
        headers: headers,
      })
      .subscribe(
        (response) => {
          this.snackBar.open('Processando nota!', 'Sucesso', {
            duration: 4000,
            panelClass: ['snackbar-success'],
          });
          this.fetchNotasFiscais();
        },
        (error) => {
          console.error('Erro ao imprimir nota:', error);
          const errorMsg = error.error?.message || 'Erro desconhecido';
          this.snackBar.open(`Erro ao imprimir: ${errorMsg}`, 'Fechar', {
            duration: 5000,
            panelClass: ['snackbar-error'],
          });
        },
      );
  }
}
