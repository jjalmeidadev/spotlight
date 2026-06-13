#!/usr/bin/env bash

set -euo pipefail

RNFS=("01" "02" "03" "04" "05" "06" "07" "08" "10" "11" "12")

REQUIRED_FILES=(
  "src/backend/tests/non-functional/README.md"
  "src/backend/tests/non-functional/.env.rnf.example"
  "src/backend/tests/non-functional/common/result-template.md"
)

for file in "${REQUIRED_FILES[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Arquivo obrigatório não encontrado: $file"
    exit 1
  fi
done

for rnf in "${RNFS[@]}"; do
  directory="src/backend/tests/non-functional/rnf${rnf}"

  if [[ ! -d "$directory" ]]; then
    echo "Pasta obrigatória não encontrada: $directory"
    exit 1
  fi
done

if [[ -d "src/backend/tests/non-functional/rnf09" ]]; then
  echo "A pasta RNF09 não deve existir, pois esse requisito não será executado."
  exit 1
fi

local_env="src/backend/tests/non-functional/.env.rnf"

if [[ -f "$local_env" ]]; then
  if ! git check-ignore --quiet "$local_env"; then
    echo "O arquivo .env.rnf existe, mas não está protegido pelo .gitignore."
    exit 1
  fi
fi

echo "Infraestrutura dos testes de RNFs validada com sucesso."
echo "RNFs aplicáveis: RNF01 a RNF08 e RNF10 a RNF12."
echo "RNF09: não aplicável nesta versão."