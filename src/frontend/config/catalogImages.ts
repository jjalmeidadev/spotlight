export const CATALOG_IMAGE = {
  whiteShell: require('../../../assets/images/catalog/component-whitebgtop.png'),
  logo: require('../../../assets/images/catalog/logo-spotlight.png'),
  searchIcon: require('../../../assets/images/catalog/searchbar-icon.png'),
  filterIcon: require('../../../assets/images/catalog/filter-config-component.png'),
} as const;

export function normalizarTitulo(titulo: string) {
  return titulo
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLocaleLowerCase('pt-BR');
}
