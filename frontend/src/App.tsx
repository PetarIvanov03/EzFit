import { Route, Routes } from "react-router-dom"
import { AppShell } from "@/components/layout/AppShell"
import { DayListPage } from "@/pages/DayListPage"
import { DayDetailPage } from "@/pages/DayDetailPage"
import { AddEntryPage } from "@/pages/AddEntryPage"

function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<DayListPage />} />
        <Route path="days/:date" element={<DayDetailPage />} />
        <Route path="add" element={<AddEntryPage />} />
      </Route>
    </Routes>
  )
}

export default App
