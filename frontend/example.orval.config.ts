import { defineConfig } from "orval";

export default defineConfig({
  petstore: {
    output: {
      mode: "split",
      target: "src/api/endpoints/petstoreFromFileSpecWithTransformer.ts",
      schemas: "src/api/model",
      client: "react-query",
      httpClient: "axios",
      mock: true,
      prettier: true,
      override: {
        mutator: {
          path: "./src/api/mutator/custom-instance.ts",
          name: "customInstance",
        },
        operations: {
          listPets: {
            query: {
              useQuery: true,
              useSuspenseQuery: true,
              useSuspenseInfiniteQuery: true,
              useInfinite: true,
              useInfiniteQueryParam: "limit",
              useInvalidate: true,
            },
          },
          showPetById: {},
        },
      },
    },
    input: {
      target: "./petstore.yaml",
      override: {
        transformer: "./src/api/transformer/add-version.js",
      },
    },
  },
});
