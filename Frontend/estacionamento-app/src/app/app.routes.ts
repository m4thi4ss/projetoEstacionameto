import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { PatioComponent } from './components/patio/patio.component';
import { VeiculosComponent } from './components/veiculos/veiculos.component';
import { RelatoriosComponent } from './components/relatorios/relatorios.component';
import { UsuariosAdminComponent } from './components/usuarios-admin/usuarios-admin.component';
import { HistoricoSessoesComponent } from './components/historico-sessoes/historico-sessoes.component';
import { authGuard, adminGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', redirectTo: '/patio', pathMatch: 'full' },
  { path: 'patio', component: PatioComponent, canActivate: [authGuard] },
  { path: 'veiculos', component: VeiculosComponent, canActivate: [authGuard] },
  { path: 'historico', component: HistoricoSessoesComponent, canActivate: [authGuard] },
  { path: 'relatorios', component: RelatoriosComponent, canActivate: [authGuard] },
  { path: 'usuarios', component: UsuariosAdminComponent, canActivate: [adminGuard] }
];
