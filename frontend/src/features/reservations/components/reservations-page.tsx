import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Plus } from "lucide-react";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setFactoryFilter, setPersonFilter } from "../store";
import { ReservationsTable } from "./reservations-table";
import { ReservationFormDialog } from "./reservation-form-dialog";
import { useReservationsPagination } from "../hooks";
import { useGetApiFactoriesAll } from "@/api/generated/factories/factories";
import { useGetApiPersonnelAll } from "@/api/generated/personnel/personnel";

export function ReservationsPage() {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const [createOpen, setCreateOpen] = useState(false);

  const factoryFilter = useAppSelector((s) => s.reservations.factoryFilter);
  const personFilter = useAppSelector((s) => s.reservations.personFilter);

  const { reservations, isLoading, page, totalPages, setPage } = useReservationsPagination();

  const { data: factoriesResponse } = useGetApiFactoriesAll();
  const { data: personnelResponse } = useGetApiPersonnelAll();
  const factories = factoriesResponse?.data?.data ?? [];
  const personnel = personnelResponse?.data?.data ?? [];

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t("reservations.title")}</h1>
          <p className="text-muted-foreground">{t("reservations.description")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t("reservations.newReservation")}
        </Button>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <Select
          value={factoryFilter !== null ? String(factoryFilter) : "all"}
          onValueChange={(v) => dispatch(setFactoryFilter(v === "all" ? null : Number(v)))}
        >
          <SelectTrigger className="w-48">
            <SelectValue placeholder={t("reservations.filter.allFactories")} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("reservations.filter.allFactories")}</SelectItem>
            {factories.map((f) => (
              <SelectItem key={f.id} value={String(f.id)}>
                {f.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          value={personFilter !== null ? String(personFilter) : "all"}
          onValueChange={(v) => dispatch(setPersonFilter(v === "all" ? null : Number(v)))}
        >
          <SelectTrigger className="w-48">
            <SelectValue placeholder={t("reservations.filter.allPersonnel")} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("reservations.filter.allPersonnel")}</SelectItem>
            {personnel.map((p) => (
              <SelectItem key={p.id} value={String(p.id)}>
                {p.fullName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <ReservationsTable
        reservations={reservations}
        isLoading={isLoading}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      <ReservationFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
