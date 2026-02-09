import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { DialogService } from '../../services/dialog.service';
import { UsuarioAuth, CriarUsuarioRequest } from '../../models/models';

@Component({
  selector: 'app-usuarios-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './usuarios-admin.component.html',
  styleUrls: ['./usuarios-admin.component.css']
})
export class UsuariosAdminComponent implements OnInit {
  usuarios: UsuarioAuth[] = [];
  usuario: CriarUsuarioRequest = { nome: '', email: '', senha: '', perfil: 1 };
  editando = false;
  usuarioEditandoId: number | null = null;
  loading = false;
  error = '';
  showForm = false;

  perfis = [
    { nome: 'Operador', valor: 1 },
    { nome: 'Admin', valor: 2 }
  ];

  constructor(
    private apiService: ApiService,
    public authService: AuthService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.carregarUsuarios();
  }

  carregarUsuarios(): void {
    this.loading = true;
    this.apiService.getUsuarios().subscribe({
      next: (data) => {
        this.usuarios = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar usuários';
        this.loading = false;
        console.error(err);
      }
    });
  }

  novoUsuario(): void {
    this.usuario = { nome: '', email: '', senha: '', perfil: 1 };
    this.editando = false;
    this.showForm = true;
  }

  editarUsuario(u: UsuarioAuth): void {
    this.usuario = { 
      nome: u.nome, 
      email: u.email, 
      senha: '', 
      perfil: u.perfil === 'Admin' ? 2 : 1 
    };
    this.editando = true;
    this.usuarioEditandoId = u.id!;
    this.showForm = true;
  }

  salvarUsuario(): void {
    if (!this.usuario.nome || !this.usuario.email) {
      this.dialogService.alert('Atenção', 'Nome e email são obrigatórios');
      return;
    }

    if (!this.editando && !this.usuario.senha) {
      this.dialogService.alert('Atenção', 'Senha é obrigatória para novo usuário');
      return;
    }

    this.loading = true;

    if (this.editando && this.usuarioEditandoId) {
      const updateData = {
        nome: this.usuario.nome,
        email: this.usuario.email,
        perfil: this.getPerfil(this.usuario.perfil),
        ativo: true
      };

      this.apiService.updateUsuario(this.usuarioEditandoId, updateData).subscribe({
        next: () => {
          this.carregarUsuarios();
          this.cancelar();
          this.dialogService.alert('Sucesso', 'Usuário atualizado com sucesso!');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao atualizar usuário';
          this.loading = false;
          this.dialogService.alert('Erro', this.error);
        }
      });
    } else {
      this.apiService.createUsuario(this.usuario).subscribe({
        next: () => {
          this.carregarUsuarios();
          this.cancelar();
          this.dialogService.alert('Sucesso', 'Usuário criado com sucesso!');
        },
        error: (err) => {
          this.error = err.error?.message || 'Erro ao criar usuário';
          this.loading = false;
          this.dialogService.alert('Erro', this.error);
        }
      });
    }
  }

  deletarUsuario(id: number, nome: string): void {
    if (id === this.authService.currentUserValue?.id) {
      this.dialogService.alert('Atenção', 'Você não pode excluir seu próprio usuário!');
      return;
    }

    this.dialogService.confirm('Excluir usuário', `Deseja realmente excluir o usuário ${nome}?`).then(ok => {
      if (!ok) return;
      this.apiService.deleteUsuario(id).subscribe({
        next: () => {
          this.carregarUsuarios();
          this.dialogService.alert('Sucesso', 'Usuário excluído com sucesso!');
        },
        error: (err) => {
          this.error = 'Erro ao excluir usuário';
          this.dialogService.alert('Erro', this.error);
          console.error(err);
        }
      });
    });
  }

  cancelar(): void {
    this.usuario = { nome: '', email: '', senha: '', perfil: 1 };
    this.editando = false;
    this.usuarioEditandoId = null;
    this.showForm = false;
    this.error = '';
  }

  getPerfil(valor: number): string {
    return valor === 2 ? 'Admin' : 'Operador';
  }
}
