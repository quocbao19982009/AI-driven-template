import { defineConfig } from "orval";

export default defineConfig({
  api: {
    input: {
      target: "../backend/swagger.json",
    },
    output: {
      mode: "tags-split",
      tsconfig: "./tsconfig.app.json",
      target: "./src/api/generated/api.ts",
      schemas: "./src/api/generated/models",
      client: "react-query",
      clean: true,
      prettier: false,
      override: {
        mutator: {
          path: "./src/api/mutator/apiFetch.ts",
          name: "apiFetch",
        },
        query: {
          useQuery: true,
          useMutation: true,
          useSuspenseQuery: false,
        },
      },
    },
  },
  apiZod: {
    input: {
      target: "../backend/swagger.json",
    },
    output: {
      mode: "tags-split",
      tsconfig: "./tsconfig.app.json",
      client: "zod",
      target: "./src/api/generated/api.ts",
      fileExtension: ".zod.ts",
      prettier: false,
    },
  },
});
