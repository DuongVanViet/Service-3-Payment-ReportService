<template>
  <div class="space-y-4">
    <div>
      <label class="block text-sm font-medium text-gray-700">Tên Tài Khoản</label>
      <input v-model="form.username" type="text" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" placeholder="username" />
    </div>

    <div>
      <label class="block text-sm font-medium text-gray-700">Email</label>
      <input v-model="form.email" type="email" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" placeholder="email@example.com" />
    </div>

    <div>
      <label class="block text-sm font-medium text-gray-700">Họ Tên</label>
      <input v-model="form.fullName" type="text" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" placeholder="Họ Tên" />
    </div>

    <div>
      <label class="block text-sm font-medium text-gray-700">Số Điện Thoại</label>
      <input v-model="form.phone" type="tel" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" placeholder="0912345678" />
    </div>

    <div>
      <label class="block text-sm font-medium text-gray-700">Chức Vụ</label>
      <select v-model="form.role" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500">
        <option value="">-- Chọn chức vụ --</option>
        <option value="Admin">Admin</option>
        <option value="Teacher">Giáo Viên</option>
        <option value="Student">Học Viên</option>
      </select>
    </div>

    <div>
      <label class="block text-sm font-medium text-gray-700">ID Liên Quan</label>
      <input v-model="form.relatedEntityId" type="number" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" placeholder="0" />
    </div>

    <div>
      <label class="block text-sm font-medium text-gray-700">Loại Thực Thể</label>
      <input v-model="form.relatedEntityType" type="text" class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500" placeholder="Student, Teacher, Admin" />
    </div>

    <div class="flex gap-4">
      <button @click="submit" class="flex-1 bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700">
        {{ isEdit ? 'Cập Nhật' : 'Tạo Tài Khoản' }}
      </button>
      <button @click="resetForm" class="flex-1 bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500">
        Hủy
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

interface Props {
  isEdit?: boolean;
  initialData?: any;
}

interface Emits {
  (e: 'submit', data: any): void;
  (e: 'cancel'): void;
}

const props = withDefaults(defineProps<Props>(), {
  isEdit: false
});

const emit = defineEmits<Emits>();

const form = ref({
  username: '',
  email: '',
  fullName: '',
  phone: '',
  role: '',
  relatedEntityId: 0,
  relatedEntityType: ''
});

const submit = () => {
  emit('submit', form.value);
};

const resetForm = () => {
  form.value = {
    username: '',
    email: '',
    fullName: '',
    phone: '',
    role: '',
    relatedEntityId: 0,
    relatedEntityType: ''
  };
  emit('cancel');
};

if (props.initialData && props.isEdit) {
  Object.assign(form.value, props.initialData);
}
</script>
