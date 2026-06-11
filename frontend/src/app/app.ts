import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

interface LoteEstoque {
  loteEstoqueId: number;
  produto: string;
  tipoProduto: string;
  tipoReceita: string;
  numeroLote: string;
  dataValidade: string;
  quantidade: number;
}

interface Retencao {
  produto: string;
  numeroLote: string;
  dataValidade: string;
  quantidade: number;
  tipoMovimento: string;
  dataMovimento: string;
  farmaceutico: string;
  receitaPaciente: string;
}

interface ItemVendaForm {
  loteEstoqueId: number | null;
  quantidade: number;
}

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private http = inject(HttpClient);

  // Base da API (.NET). Em produção a API serve este build a partir de wwwroot,
  // então usamos caminho relativo "/api"; em desenvolvimento (ng serve) apontamos
  // direto para o backend rodando localmente.
  apiBase = '/api';

  // ----- Entrada (RF01-RF04) -----
  arquivoXml: File | null = null;
  entFarm = '';
  entSenha = '';
  entPaciente = '';
  entReceita = '';
  msgEntrada = '';
  msgEntradaOk = false;

  // ----- Estoque (RF04 / RF05) -----
  estoque: LoteEstoque[] = [];
  vencimentos: LoteEstoque[] = [];

  // ----- Vendas (RF03 / RF07) -----
  itensVenda: ItemVendaForm[] = [{ loteEstoqueId: null, quantidade: 1 }];
  vdFarm = '';
  vdSenha = '';
  vdPaciente = '';
  vdReceita = '';
  msgVenda = '';
  msgVendaOk = false;

  // ----- SNGPC (RF06) -----
  retencoes: Retencao[] = [];
  msgSngpc = '';
  msgSngpcOk = false;

  // ----- Login / JWT (RF03 / RNF03) -----
  loginNome = '';
  loginSenha = '';
  token: string | null = null;
  usuarioLogado: string | null = null;
  msgLogin = '';
  msgLoginOk = false;

  constructor() {
    this.carregarEstoque();
  }

  // RF03: autentica o farmacêutico e obtém o token JWT.
  // RNF03: esse token é exigido para acessar os dados sensíveis do SNGPC.
  async login() {
    try {
      const resp: any = await firstValueFrom(
        this.http.post(`${this.apiBase}/auth/login`, {
          nome: this.loginNome,
          senha: this.loginSenha
        })
      );
      this.token = resp.token;
      this.usuarioLogado = resp.nome;
      this.msgLoginOk = true;
      this.msgLogin = `Login realizado com sucesso (${resp.nome}). Token JWT obtido.`;
    } catch (err: any) {
      this.token = null;
      this.usuarioLogado = null;
      this.msgLoginOk = false;
      this.msgLogin = this.extrairErro(err);
    }
  }

  logout() {
    this.token = null;
    this.usuarioLogado = null;
    this.retencoes = [];
    this.msgLogin = '';
  }

  private authHeaders(): HttpHeaders {
    return new HttpHeaders(this.token ? { Authorization: `Bearer ${this.token}` } : {});
  }

  onArquivoSelecionado(event: Event) {
    const input = event.target as HTMLInputElement;
    this.arquivoXml = input.files && input.files.length > 0 ? input.files[0] : null;
  }

  // RF01: importa o XML da NF-e
  async importarXml() {
    if (!this.arquivoXml) {
      this.msgEntradaOk = false;
      this.msgEntrada = 'Selecione um arquivo XML.';
      return;
    }

    const form = new FormData();
    form.append('ArquivoXml', this.arquivoXml);
    form.append('FarmaceuticoNome', this.entFarm);
    form.append('FarmaceuticoSenha', this.entSenha);
    form.append('NomePaciente', this.entPaciente);
    form.append('NumeroReceita', this.entReceita);

    try {
      const resp: any = await firstValueFrom(
        this.http.post(`${this.apiBase}/entrada/importar-xml`, form)
      );
      this.msgEntradaOk = true;
      this.msgEntrada = `Nota ${resp.numeroNota} importada. Itens: ${resp.itensProcessados.join(' | ')}`;
      await this.carregarEstoque();
    } catch (err: any) {
      this.msgEntradaOk = false;
      this.msgEntrada = this.extrairErro(err);
    }
  }

  // RF04: saldo atual em estoque
  async carregarEstoque() {
    this.estoque = await firstValueFrom(this.http.get<LoteEstoque[]>(`${this.apiBase}/estoque`));
  }

  // RF05: alerta de vencimento (30 dias)
  async carregarVencimentos() {
    this.vencimentos = await firstValueFrom(
      this.http.get<LoteEstoque[]>(`${this.apiBase}/estoque/vencimentos?dias=30`)
    );
  }

  // ----- PDV / Vendas (RF03, RF07) -----
  adicionarItemVenda() {
    this.itensVenda.push({ loteEstoqueId: null, quantidade: 1 });
  }

  removerItemVenda(index: number) {
    this.itensVenda.splice(index, 1);
  }

  async confirmarVenda() {
    const itens = this.itensVenda
      .filter(i => i.loteEstoqueId !== null)
      .map(i => ({ loteEstoqueId: i.loteEstoqueId, quantidade: i.quantidade }));

    if (itens.length === 0) {
      this.msgVendaOk = false;
      this.msgVenda = 'Adicione ao menos um item.';
      return;
    }

    const payload = {
      itens,
      farmaceuticoNome: this.vdFarm || null,
      farmaceuticoSenha: this.vdSenha || null,
      nomePaciente: this.vdPaciente || null,
      numeroReceita: this.vdReceita || null
    };

    try {
      await firstValueFrom(this.http.post(`${this.apiBase}/vendas`, payload));
      this.msgVendaOk = true;
      this.msgVenda = 'Venda confirmada com sucesso! Estoque atualizado.';
      this.itensVenda = [{ loteEstoqueId: null, quantidade: 1 }];
      await this.carregarEstoque();
    } catch (err: any) {
      this.msgVendaOk = false;
      this.msgVenda = this.extrairErro(err);
    }
  }

  // ----- SNGPC (RF06 / RNF03 - requer JWT) -----
  async carregarRetencoes() {
    if (!this.token) {
      this.msgSngpcOk = false;
      this.msgSngpc = 'Faça login como farmacêutico para acessar os dados do SNGPC.';
      return;
    }
    try {
      this.retencoes = await firstValueFrom(
        this.http.get<Retencao[]>(`${this.apiBase}/sngpc/retencoes?somentePendentes=true`, {
          headers: this.authHeaders()
        })
      );
    } catch (err: any) {
      this.msgSngpcOk = false;
      this.msgSngpc = this.extrairErro(err);
    }
  }

  async gerarTransmissao() {
    if (!this.token) {
      this.msgSngpcOk = false;
      this.msgSngpc = 'Faça login como farmacêutico para gerar o arquivo do SNGPC.';
      return;
    }
    try {
      const resp = await fetch(`${this.apiBase}/sngpc/transmissao`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${this.token}` }
      });

      if (resp.status === 204) {
        this.msgSngpcOk = false;
        this.msgSngpc = 'Não há retenções pendentes para transmitir.';
        return;
      }

      const blob = await resp.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      const cd = resp.headers.get('Content-Disposition') || '';
      const nome = cd.split('filename=')[1]?.replaceAll('"', '') || 'sngpc.txt';
      a.download = nome;
      a.click();
      URL.revokeObjectURL(url);

      this.msgSngpcOk = true;
      this.msgSngpc = 'Arquivo de transmissão gerado e baixado.';
      await this.carregarRetencoes();
    } catch (err: any) {
      this.msgSngpcOk = false;
      this.msgSngpc = String(err);
    }
  }

  private extrairErro(err: any): string {
    if (err?.error) {
      return typeof err.error === 'string' ? err.error : JSON.stringify(err.error);
    }
    return err?.message || 'Erro desconhecido';
  }
}
