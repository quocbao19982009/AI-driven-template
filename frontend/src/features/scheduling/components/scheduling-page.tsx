import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import {
  useGetApiSchedulingByPerson,
  useGetApiSchedulingByFactory,
} from "@/api/generated/scheduling/scheduling";

type Tab = "by-person" | "by-factory";

export function SchedulingPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<Tab>("by-person");

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t("scheduling.title")}</h1>
        <p className="text-muted-foreground">{t("scheduling.description")}</p>
      </div>

      <div className="flex gap-1 border rounded-md p-1 w-fit">
        <Button
          size="sm"
          variant={activeTab === "by-person" ? "default" : "ghost"}
          onClick={() => setActiveTab("by-person")}
        >
          {t("scheduling.tabs.byPerson")}
        </Button>
        <Button
          size="sm"
          variant={activeTab === "by-factory" ? "default" : "ghost"}
          onClick={() => setActiveTab("by-factory")}
        >
          {t("scheduling.tabs.byFactory")}
        </Button>
      </div>

      {activeTab === "by-person" ? <ByPersonTab t={t} /> : <ByFactoryTab t={t} />}
    </div>
  );
}

function ByPersonTab({ t }: { t: (key: string, opts?: Record<string, unknown>) => string }) {
  const { data: response, isLoading } = useGetApiSchedulingByPerson();
  const persons = response?.data?.data ?? [];

  if (isLoading) {
    return (
      <div className="space-y-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-24 w-full" />
        ))}
      </div>
    );
  }

  if (persons.length === 0) {
    return (
      <p className="text-muted-foreground text-center py-12">{t("scheduling.byPerson.empty")}</p>
    );
  }

  return (
    <div className="space-y-4">
      {persons.map((person) => (
        <div key={person.personId ?? person.personName} className="border rounded-lg p-4">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center gap-2">
              <h3 className="font-semibold">{person.personName}</h3>
              {person.personId === null && (
                <Badge className="bg-gray-100 text-gray-600 text-xs">{t("scheduling.deleted")}</Badge>
              )}
            </div>
            <Badge className="bg-blue-100 text-blue-700">
              {t("scheduling.totalHours", { hours: person.totalHours?.toFixed(1) })}
            </Badge>
          </div>

          {person.reservations && person.reservations.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t("scheduling.byPerson.factory")}</TableHead>
                  <TableHead>{t("scheduling.byPerson.startTime")}</TableHead>
                  <TableHead>{t("scheduling.byPerson.endTime")}</TableHead>
                  <TableHead>{t("scheduling.byPerson.duration")}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {person.reservations.map((res) => (
                  <TableRow key={res.reservationId}>
                    <TableCell>{res.factoryName}</TableCell>
                    <TableCell className="text-muted-foreground">
                      {new Date(res.startTime).toLocaleString()}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {new Date(res.endTime).toLocaleString()}
                    </TableCell>
                    <TableCell>{res.durationHours?.toFixed(1)}h</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <p className="text-sm text-muted-foreground">{t("scheduling.byPerson.noReservations")}</p>
          )}
        </div>
      ))}
    </div>
  );
}

function ByFactoryTab({ t }: { t: (key: string, opts?: Record<string, unknown>) => string }) {
  const { data: response, isLoading } = useGetApiSchedulingByFactory();
  const factories = response?.data?.data ?? [];

  if (isLoading) {
    return <Skeleton className="h-48 w-full" />;
  }

  if (factories.length === 0) {
    return (
      <p className="text-muted-foreground text-center py-12">{t("scheduling.byFactory.empty")}</p>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t("scheduling.byFactory.factory")}</TableHead>
            <TableHead>{t("scheduling.byFactory.reservationCount")}</TableHead>
            <TableHead>{t("scheduling.byFactory.totalHours")}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {factories.map((factory) => (
            <TableRow key={factory.factoryId ?? factory.factoryName}>
              <TableCell className="font-medium">
                <div className="flex items-center gap-2">
                  {factory.factoryName}
                  {factory.factoryId === null && (
                    <Badge className="bg-gray-100 text-gray-600 text-xs">{t("scheduling.deleted")}</Badge>
                  )}
                </div>
              </TableCell>
              <TableCell>{factory.reservationCount}</TableCell>
              <TableCell>{factory.totalHours?.toFixed(1)}h</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
