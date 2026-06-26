// UUIDs de usuarios reais do seed (02_initial_data.sql). Idem em auth.users e
// public.usuario via trigger on_auth_user_created. Usados como claim `sub` para
// montar JWTs locais; o servico de recomendacao busca por id_usuario.

export const SEED_USERS = [
  { sub: '00000000-0000-0000-0000-000000000002', email: 'ana.clara@email.com' },
  { sub: '00000000-0000-0000-0000-000000000003', email: 'carlos.nobrega@email.com' },
  { sub: '00000000-0000-0000-0000-000000000004', email: 'mariana.lacerda@email.com' },
  { sub: '00000000-0000-0000-0000-000000000005', email: 'pedro.lima@email.com' },
  { sub: '00000000-0000-0000-0000-000000000006', email: 'juliana.santos@email.com' },
  { sub: '00000000-0000-0000-0000-000000000007', email: 'rodrigo.alves@email.com' },
  { sub: '00000000-0000-0000-0000-000000000008', email: 'beatriz.melo@email.com' },
  { sub: '00000000-0000-0000-0000-000000000009', email: 'thiago.costa@email.com' },
  { sub: '00000000-0000-0000-0000-000000000010', email: 'camila.ferreira@email.com' },
  { sub: '00000000-0000-0000-0000-000000000011', email: 'lucas.oliveira@email.com' },
  { sub: '00000000-0000-0000-0000-000000000012', email: 'amanda.ribeiro@email.com' },
  { sub: '00000000-0000-0000-0000-000000000013', email: 'gustavo.souza@email.com' },
  { sub: '00000000-0000-0000-0000-000000000014', email: 'fernanda.lima@email.com' },
  { sub: '00000000-0000-0000-0000-000000000015', email: 'rafael.carvalho@email.com' },
  { sub: '00000000-0000-0000-0000-000000000016', email: 'priscila.mendes@email.com' },
  { sub: '00000000-0000-0000-0000-000000000017', email: 'eduardo.nunes@email.com' },
  { sub: '00000000-0000-0000-0000-000000000018', email: 'leticia.rocha@email.com' },
  { sub: '00000000-0000-0000-0000-000000000019', email: 'vinicius.martins@email.com' },
  { sub: '00000000-0000-0000-0000-000000000020', email: 'carolina.andrade@email.com' },
  { sub: '00000000-0000-0000-0000-000000000021', email: 'felipe.gomes@email.com' },
  { sub: '00000000-0000-0000-0000-000000000022', email: 'natalia.freitas@email.com' },
  { sub: '00000000-0000-0000-0000-000000000023', email: 'andre.barbosa@email.com' },
  { sub: '00000000-0000-0000-0000-000000000024', email: 'paula.teixeira@email.com' },
  { sub: '00000000-0000-0000-0000-000000000025', email: 'marcos.pereira@email.com' },
];

export function pickUser(idx) {
  return SEED_USERS[idx % SEED_USERS.length];
}
