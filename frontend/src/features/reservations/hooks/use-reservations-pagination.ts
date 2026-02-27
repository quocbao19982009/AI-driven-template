import { useState, useEffect } from "react";
import { useGetApiReservations } from "@/api/generated/reservations/reservations";
import { useAppSelector } from "@/store/hooks";

const DEFAULT_PAGE_SIZE = 10;

export function useReservationsPagination(pageSize = DEFAULT_PAGE_SIZE) {
  const [page, setPage] = useState(1);

  const factoryFilter = useAppSelector((s) => s.reservations.factoryFilter);
  const personFilter = useAppSelector((s) => s.reservations.personFilter);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setPage(1);
  }, [factoryFilter, personFilter]);

  const { data: response, isLoading } = useGetApiReservations({
    page,
    pageSize,
    factoryId: factoryFilter ?? undefined,
    personId: personFilter ?? undefined,
  });

  const pagedResult = response?.data?.data;

  return {
    reservations: pagedResult?.items ?? [],
    totalPages: pagedResult?.totalPages ?? 1,
    page,
    setPage,
    isLoading,
  };
}
