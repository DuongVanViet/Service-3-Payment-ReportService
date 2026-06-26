<template>
  <div class="container mx-auto p-6">
    <div class="mb-6">
      <h1 class="text-3xl font-bold mb-2">Dashboard Giáo Viên</h1>
      <p class="text-gray-600">Xem tổng quan các lớp và báo cáo của bạn</p>
    </div>

    <!-- Summary -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-blue-50 p-4 rounded-lg border-l-4 border-blue-500">
        <p class="text-gray-600 text-sm">Số Lớp Dạy</p>
        <p class="text-2xl font-bold text-blue-600">{{ summary.classCount }}</p>
      </div>
      <div class="bg-green-50 p-4 rounded-lg border-l-4 border-green-500">
        <p class="text-gray-600 text-sm">Tổng Học Viên</p>
        <p class="text-2xl font-bold text-green-600">{{ summary.totalStudents }}</p>
      </div>
      <div class="bg-orange-50 p-4 rounded-lg border-l-4 border-orange-500">
        <p class="text-gray-600 text-sm">Vắng Nhiều</p>
        <p class="text-2xl font-bold text-orange-600">{{ summary.attendanceRiskCount }}</p>
      </div>
      <div class="bg-purple-50 p-4 rounded-lg border-l-4 border-purple-500">
        <p class="text-gray-600 text-sm">Chưa Nhập Điểm</p>
        <p class="text-2xl font-bold text-purple-600">{{ summary.pendingGrades }}</p>
      </div>
    </div>

    <!-- Tabs -->
    <div class="mb-6 border-b border-gray-300">
      <div class="flex gap-4">
        <button 
          v-for="tab in tabs" 
          :key="tab"
          @click="activeTab = tab"
          :class="[
            'px-4 py-2 font-semibold border-b-2 transition-colors',
            activeTab === tab 
              ? 'border-blue-600 text-blue-600' 
              : 'border-transparent text-gray-600 hover:text-gray-800'
          ]"
        >
          {{ tabLabels[tab] }}
        </button>
      </div>
    </div>

    <!-- Classes List Tab -->
    <div v-if="activeTab === 'classes'" class="space-y-4">
      <div v-for="classItem in classes" :key="classItem.id" class="bg-white p-4 rounded-lg shadow hover:shadow-md transition-shadow cursor-pointer" @click="selectClass(classItem)">
        <div class="flex justify-between items-start mb-2">
          <div>
            <h3 class="font-bold text-lg">{{ classItem.name }}</h3>
            <p class="text-gray-600">Khóa: {{ classItem.courseId }} | Lớp: {{ classItem.classId }}</p>
          </div>
          <StatusBadge :status="classItem.status" type="user" />
        </div>
        <div class="grid grid-cols-4 gap-4 text-sm">
          <div>
            <p class="text-gray-600">Học Viên</p>
            <p class="font-bold">{{ classItem.studentCount }}</p>
          </div>
          <div>
            <p class="text-gray-600">Chuyên Cần TB</p>
            <p class="font-bold">{{ classItem.avgAttendance }}%</p>
          </div>
          <div>
            <p class="text-gray-600">Vắng Nhiều</p>
            <p class="font-bold text-orange-600">{{ classItem.attendanceRiskCount }}</p>
          </div>
          <div>
            <p class="text-gray-600">Chưa Điểm</p>
            <p class="font-bold text-red-600">{{ classItem.pendingGradeCount }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Reports Tab -->
    <div v-if="activeTab === 'reports'" class="space-y-4">
      <div class="bg-white p-4 rounded-lg shadow">
        <h3 class="font-bold text-lg mb-4">Báo Cáo Lớp</h3>
        <div class="space-y-3">
          <div v-for="report in reports" :key="report.id" class="border-l-4 border-blue-500 p-3 bg-blue-50">
            <p class="font-semibold">{{ report.className }}</p>
            <div class="grid grid-cols-3 gap-4 text-sm mt-2">
              <div>
                <p class="text-gray-600">Tỷ Lệ Chuyên Cần</p>
                <p class="font-bold">{{ report.attendanceRate }}%</p>
              </div>
              <div>
                <p class="text-gray-600">Điểm Trung Bình</p>
                <p class="font-bold">{{ report.avgScore }}</p>
              </div>
              <div>
                <p class="text-gray-600">Đạt Yêu Cầu</p>
                <p class="font-bold text-green-600">{{ report.passCount }}/{{ report.totalStudents }}</p>
              </div>
            </div>
            <button @click="viewClassReport(report.id)" class="mt-2 text-blue-600 hover:underline text-sm">Chi Tiết →</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Class Detail Modal -->
    <div v-if="selectedClass" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white p-6 rounded-lg max-w-2xl w-full max-h-96 overflow-y-auto">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-xl font-bold">{{ selectedClass.name }}</h2>
          <button @click="selectedClass = null" class="text-gray-500 hover:text-gray-700">✕</button>
        </div>
        
        <div class="space-y-4">
          <div>
            <h3 class="font-semibold mb-2">Học Viên ({{ selectedClass.studentCount }})</h3>
            <div class="space-y-2 max-h-48 overflow-y-auto">
              <div v-for="student in selectedClass.students" :key="student.id" class="flex justify-between p-2 border-b">
                <span>{{ student.name }}</span>
                <span class="text-sm text-gray-600">Chuyên Cần: {{ student.attendance }}%</span>
              </div>
            </div>
          </div>
        </div>

        <div class="mt-4 flex gap-2">
          <button class="flex-1 bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700">Xem Chi Tiết</button>
          <button @click="selectedClass = null" class="flex-1 bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500">Đóng</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../api';
import StatusBadge from '../components/StatusBadge.vue';

const activeTab = ref('classes');
const tabs = ['classes', 'reports'];
const tabLabels = {
  classes: 'Các Lớp Của Tôi',
  reports: 'Báo Cáo'
};

const classes = ref([]);
const reports = ref([]);
const selectedClass = ref(null);

const summary = ref({
  classCount: 0,
  totalStudents: 0,
  attendanceRiskCount: 0,
  pendingGrades: 0
});

const loadTeacherData = async () => {
  try {
    // Load classes
    const classesResponse = await api.get('/teacher/classes');
    const classesData = classesResponse.data;
    classes.value = classesData.map((c: any) => ({
      ...c,
      studentCount: Math.floor(Math.random() * 50) + 20,
      avgAttendance: Math.floor(Math.random() * 20) + 75,
      attendanceRiskCount: Math.floor(Math.random() * 5),
      pendingGradeCount: Math.floor(Math.random() * 10),
      status: 'Active',
      students: Array(10).fill(0).map((_, i) => ({
        id: i,
        name: `Học Viên ${i + 1}`,
        attendance: Math.floor(Math.random() * 30) + 60
      }))
    }));

    // Load reports
    const reportsResponse = await api.get('/teacher/reports/classes');
    const reportsData = reportsResponse.data;
    reports.value = reportsData || [];

    calculateSummary();
  } catch (error) {
    console.error('Error loading teacher data:', error);
    // Mock data for demo
    classes.value = [
      { id: 1, name: 'Lớp A1', courseId: 1, classId: 1, studentCount: 30, avgAttendance: 85, attendanceRiskCount: 3, pendingGradeCount: 5, status: 'Active', students: [] },
      { id: 2, name: 'Lớp B2', courseId: 1, classId: 2, studentCount: 25, avgAttendance: 78, attendanceRiskCount: 4, pendingGradeCount: 8, status: 'Active', students: [] }
    ];
  }
};

const calculateSummary = () => {
  summary.value = {
    classCount: classes.value.length,
    totalStudents: classes.value.reduce((sum, c) => sum + c.studentCount, 0),
    attendanceRiskCount: classes.value.reduce((sum, c) => sum + c.attendanceRiskCount, 0),
    pendingGrades: classes.value.reduce((sum, c) => sum + c.pendingGradeCount, 0)
  };
};

const selectClass = (classItem: any) => {
  selectedClass.value = classItem;
};

const viewClassReport = (reportId: number) => {
  console.log('View report:', reportId);
};

onMounted(() => {
  loadTeacherData();
});
</script>
